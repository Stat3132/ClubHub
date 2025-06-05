const express = require('express');
const axios = require('axios');
const crypto = require('crypto'); 
const router = express.Router();
const authorizeRoles = require('../middleware/authorizeRoles');

//USER FUNCTIONALITY ROUTES/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// signup page
router.get('/signup', (req, res) => {
  res.render('signup');
});

router.post('/signup', async (req, res) => {
  const { firstName, lastName, email, phoneNumber, password, role } = req.body;

  try {
    const response = await axios.post('http://PRO290UserServiceAPI:8080/api/users', {
      firstName,
      lastName,
      email,
      phoneNumber,
      password,
      role
    });

    if (response.data.success) {
      
      return res.redirect('/'); 
    } else {
      
      return res.render('signup', { error: response.data.Message });
    }
  } catch (err) {
    console.error('Signup error:', err.message);
    return res.status(500).render('signup', { error: 'Signup failed. Try again.' });
  }
});

// login page
router.get('/', (req, res) => {
  res.render('login');
});

const jwt = require('jsonwebtoken');
const sql = require('mssql');

router.post('/login', async (req, res) => {
  const { email, password } = req.body;

  try {
    // Step 1: Authenticate with UserService and get JWT
    const response = await axios.post('http://PRO290UserServiceAPI:8080/api/users/login', {
      Email: email,
      Password: password
    });

    const data = response.data;

    if (!data.success || !data.token) {
      return res.render('login', { error: data.message || 'Invalid login.' });
    }

    // Step 2: Store the JWT token in a cookie
    res.cookie('token', data.token, {
      httpOnly: true,
      secure: process.env.NODE_ENV === 'production',
      maxAge: 60 * 60 * 1000 // 1 hour
    });

    // Step 3: Decode token to get the roles and userID
    const decoded = jwt.decode(data.token);
    console.log('Decoded JWT:', decoded);

    const role = decoded?.role?.toLowerCase()
               || decoded?.Role?.toLowerCase()
               || decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']?.toLowerCase();

    const userID = decoded?.sub;

    if (!role) {
      console.warn('⚠️ Role claim not found in decoded JWT:', decoded);
      return res.status(403).render('login', { error: 'Access denied: no role found in token.' });
    }

    console.log(`✅ ${email} logged in as ${role}`);

    // Step 4: Role-based redirect logic
    if (role === 'admin' || role == 'advisor') {
      return res.redirect('/manage');
    }

    // Default redirect for other roles Students and presidents and what not
    return res.redirect('/home');

  } catch (err) {
    console.error('Login error:', err.response?.data || err.message);
    return res.status(401).render('login', { error: 'Login failed. Check your credentials.' });
  }
});

router.get('/home', async (req, res) => {
  const token = req.cookies.token;

  try {
    const pool = await sql.connect(sqlConfig);

    // Fetch all clubs
    const clubsResult = await pool.request().query(`
      SELECT clubID, clubName, clubDeclaration, presidentName FROM club
    `);

    // Fetch all user-club relationships with user info
    const membersResult = await pool.request().query(`
      SELECT uc.clubID, u.firstName, u.lastName, u.email
      FROM userclub uc
      JOIN [user] u ON uc.userID = u.userID
    `);

    const memberMap = {};
    for (const row of membersResult.recordset) {
  const cleanedClubID = row.clubID.replace(/[{}]/g, ''); // remove curly braces

  if (!memberMap[cleanedClubID]) {
    memberMap[cleanedClubID] = [];
  }

  memberMap[cleanedClubID].push({
    name: `${row.firstName} ${row.lastName}`,
    email: row.email
  });
}

    // Attach members to each club
    const clubs = clubsResult.recordset.map(club => {
  const cleanedID = club.clubID.replace(/[{}]/g, '');
  return {
    ...club,
    members: memberMap[cleanedID] || []
  };
});


    res.render('home', { clubs });

  } catch (err) {
    console.error('Error fetching clubs or members:', err.message);
    res.render('home', { clubs: [] });
  }
});

//CREATE CLUB
router.get('/create-club', (req, res) => {
  res.render('createclub', { success: false, clubName: null, error: null });
});


router.post('/create-club', authorizeRoles(['admin', 'advisor', 'student']), async (req, res) => {
  const { clubName, clubDeclaration, studentName, studentEmail, reasonToCreate } = req.body;

  try {
    const response = await axios.post('http://PRO290ClubManagementServiceAPI:8080/api/ClubManager/create-request', {
  ClubName: clubName,
  ClubDeclaration: clubDeclaration,
  StudentName: studentName,
  StudentEmail: studentEmail,
  ReasonToCreate: reasonToCreate
});

    if (response.status === 200 && response.data.success) {
      return res.render('createclub', {
        success: true,
        clubName
      });
    } else {
      return res.render('createclub', { error: response.data.Message || 'Failed to submit request.' });
    }

  } catch (err) {
    console.error('Error submitting create request:', err.message);
    return res.status(500).render('createclub', {
      error: 'Failed to submit club request. ' + err.message
    });
  }
});

//JOIN CLUB
router.get('/join-club', (req, res) => {
  res.render('join-club');
});

router.post('/join-club', authorizeRoles(['admin', 'advisor', 'student']), async (req, res) => {
  const { clubID, studentName, studentEmail, reasonToJoin } = req.body;

  try {
    const response = await axios.post('http://PRO290ClubManagementServiceAPI:8080/api/ClubManager/join-request', {
      clubID,
      studentName,
      studentEmail,
      reasonToJoin
    });

    if (response.status === 200 && response.data.success) {
      return res.render('join-club', { success: true });
    } else {
      return res.render('join-club', { error: response.data.Message || 'Failed to submit join request.' });
    }
  } catch (err) {
    console.error('Error submitting join request:', err.message);
    return res.status(500).render('join-club', {
      error: 'Failed to submit join request. ' + err.message
    });
  }
});




//ADMINSTRATION ROUTES/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
router.post('/addMember', async (req, res) => {
  const { userID, clubID } = req.body;

  try {
    const pool = await sql.connect(sqlConfig);
    await pool.request()
      .input('userID', sql.UniqueIdentifier, userID)
      .input('clubID', sql.UniqueIdentifier, clubID)
      .query('INSERT INTO userclub (userID, clubID) VALUES (@userID, @clubID)');

    res.redirect(`/manage/${clubID}`);
  } catch (err) {
    console.error('Error adding member:', err.message);
    res.status(500).send('Failed to add member');
  }
});

router.get('/manage', async (req, res) => {
  try {
    const pool = await sql.connect(sqlConfig);

    // 1. Get all users
    const usersResult = await pool.request().query(`
      SELECT userID, firstName, lastName, email, role 
      FROM [user]
    `);

    // 2. Get all user-club relationships with club names
    const userClubResult = await pool.request().query(`
      SELECT uc.userID, uc.clubID, c.clubName
      FROM userclub uc
      JOIN club c ON uc.clubID = c.clubID
    `);

    // 3. Group clubs by userID
    const userClubMap = {};
    userClubResult.recordset.forEach(({ userID, clubID, clubName }) => {
      if (!userClubMap[userID]) userClubMap[userID] = [];
      userClubMap[userID].push({ clubID, clubName });
    });

    // 4. Annotate each user with their associated clubs
    const users = usersResult.recordset.map(user => ({
      ...user,
      clubs: userClubMap[user.userID] || []
    }));

    // 5. Render admin-manage view with full user + club data
    res.render('admin-manage', { users });

  } catch (err) {
    console.error('Error loading admin manage:', err.message);
    res.status(500).send('Failed to load admin management page');
  }
});

// ROUTES (in index.js or admin.js router)
router.get('/approveClubRequest', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  try {
    const response = await axios.get('http://PRO290ClubManagementServiceAPI:8080/api/ClubManager/create-requests', {
      headers: {
        Authorization: `Bearer ${req.cookies.token}`
      }
    });

    // The API returns an array, not { requests: [...] }
    const requestArray = response.data;

    res.render('approveClubRequest', { requests: requestArray }); // ✅ correctly named `requests`
  } catch (err) {
    console.error('Error fetching create requests:', err.message);
    res.render('approveClubRequest', { requests: [] }); // provide empty fallback
  }
});

router.post('/approve-create-request/:id', async (req, res) => {
  const createRequestID = req.params.id;
  const token = req.cookies.token;

  try {
    const response = await axios.post(
      `http://PRO290OcelotAPIGateway:8080/clubmanageserviceapi/api/ClubManager/accept-create-request/${createRequestID}?advisorEmail=AdvisorLana@Advisor.com`,
      {},
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );

    res.redirect('/manage?approved=true');
  } catch (err) {
    console.error('❌ Error approving club request:', err.message);
    res.redirect('/manage?error=true');
  }
});

router.post('/deny-create-request/:id', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  const createRequestID = req.params.id;
  const token = req.cookies.token;

  try {
    const response = await axios.post(
      `http://PRO290ClubManagementServiceAPI:8080/api/ClubManager/deny-create-request/${createRequestID}`,
      {}, // No body needed
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );

    if (response.data?.success) {
      return res.redirect('/approveClubRequest');
    } else {
      return res.status(400).send('Failed to deny request: ' + response.data?.Message);
    }
  } catch (err) {
    console.error('❌ Error denying club request:', err.message);
    return res.status(500).send('Failed to deny request.');
  }
});



router.get('/removeMember', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  try {
    const pool = await sql.connect(sqlConfig);
    const result = await pool.request().query(`
      SELECT u.userID, u.firstName, u.lastName, u.email, c.clubID, c.clubName
      FROM userclub uc
      JOIN [user] u ON uc.userID = u.userID
      JOIN club c ON uc.clubID = c.clubID
    `);

    const members = result.recordset.map(row => ({
      userID: row.userID,
      clubID: row.clubID,
      name: `${row.firstName} ${row.lastName}`,
      email: row.email,
      clubName: row.clubName
    }));

    res.render('removeMember', { members }); // ✅ success path
  } catch (err) {
    console.error('Error loading remove member page:', err.message);
    res.render('removeMember', { members: [] }); // ✅ FIXED: always define `members`
  }
});


router.post('/removeMember', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  const { userID, clubID } = req.body;
  const token = req.cookies.token;

  try {
    const response = await axios.delete(
      `http://PRO290ClubManagementServiceAPI:8080/api/ClubManager/userclub`,
      {
        headers: { Authorization: `Bearer ${token}` },
        params: { userID, clubID }
      }
    );

    if (response.data?.success) {
      return res.redirect('/removeMember');
    } else {
      return res.status(400).send('Failed to remove member: ' + (response.data?.Message || ''));
    }
  } catch (err) {
    console.error('Error removing member:', err.message);
    return res.status(500).send('Internal server error.');
  }
});

router.get('/addEvent', (req, res) => {
  res.render('addEvent');
});

router.post('/addEvent', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  // Parse form data from req.body
  const {
    EventName,
    eventDescription,
    eventBudget,
    eventCoordinator,
    eventCoordinatorsNumber
  } = req.body;

  const token = req.cookies.token;

  // Build the request body for the backend
  const eventBody = {
    EventName,
    eventDescription,
    eventBudget,
    eventCoordinator,
    eventCoordinatorsNumber
  };

  try {
    const response = await axios.post(
      'http://PRO290OcelotAPIGateway:8080/eventserviceapi/createevent',
      eventBody,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );

    if (response.data?.id) {
      res.send('Event added successfully.');
    } else {
      res.status(400).send(response.data?.message || 'Failed to add event.');
    }
  } catch (err) {
    console.error('Error adding event:', err.message);
    res.status(500).send('Failed to add event.');
  }
});

router.post('/remove-user', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  console.log('🔔 /remove-user route hit');

  let { userID, userName } = req.body;

  if (!userID || typeof userID !== 'string') {
    console.log(`error ${userID}, ${userName}`)
    return res.status(400).json({ success: false, message: 'Invalid or missing user ID.' });
  }

  // Clean the ID
  userID = userID.replace(/[{}]/g, '');

  if (userID.length !== 36) {
    return res.status(400).json({ success: false, message: 'User ID must be 36 characters long after cleaning.' });
  }

  const token = req.cookies.token;

  // Print the token for debugging
  console.log('JWT token:', token);
  console.log(`UserID ${userID} \n username: ${userName}`)
  console.log(JSON.stringify(req.body, null, 2))

  try {
    const response = await axios.delete(`http://PRO290OcelotAPIGateway:8080/userserviceapi/api/users/${userID}`, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });

    if (response.data?.success) {
      return res.json({ success: true, message: `User '${userName}' was removed.` });
    } else {
      return res.status(400).json({
        success: false,
        message: response.data?.Message || `Failed to remove user '${userName}'.`
      });
    }

  } catch (err) {
    console.error('Error removing user:', {
      status: err.response?.status,
      data: err.response?.data,
      headers: err.response?.headers,
      message: err.message
    });
    return res.status(err.response?.status || 500).json({
      success: false,
      message: `Failed to remove user '${userName}'.`
    });
  }
});

// CLUB Join Requests Route ////////////////////////////////////////////////////////////////////
router.get('/approveJoinRequests', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  try {
    const token = req.cookies.token;
    const response = await axios.get('http://PRO290ClubManagementServiceAPI:8080/api/ClubManager/join-requests', {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });

    res.render('approveJoinRequest', { requests: response.data });
  } catch (err) {
    console.error('Error fetching join requests:', err.message);
    res.render('approveJoinRequest', { requests: [] });
  }
});

router.post('/approve-join-request/:id', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  const token = req.cookies.token;
  const id = req.params.id;

  try {
    await axios.post(`http://PRO290ClubManagementServiceAPI:8080/api/ClubManager/accept-join-request/${id}`, {}, {
      headers: { Authorization: `Bearer ${token}` }
    });

    res.redirect('/approveJoinRequests');
  } catch (err) {
    console.error('Error approving join request:', err.message);
    res.redirect('/approveJoinRequests');
  }
});

router.post('/deny-join-request/:id', authorizeRoles(['admin', 'advisor']), async (req, res) => {
  const token = req.cookies.token;
  const id = req.params.id;

  try {
    await axios.post(`http://PRO290ClubManagementServiceAPI:8080/api/ClubManager/deny-join-request/${id}`, {}, {
      headers: { Authorization: `Bearer ${token}` }
    });

    res.redirect('/approveJoinRequests');
  } catch (err) {
    console.error('Error denying join request:', err.message);
    res.redirect('/approveJoinRequests');
  }
});

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////





// END OF ADMIN ROUTES /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//
const sqlConfig = {
  user: 'sa',
  password: 'abc12345!',
  database: 'clubhub',
  server: 'PRO290UserClubServiceDBSqlServer',
  port: 1433,
  options: {
    encrypt: false, // if using self-hosted SQL Server (true for Azure)
    trustServerCertificate: true
  }
};

async function getClubUserDetails(presidentEmail, advisorEmail) {
  try {
    if (!presidentEmail || !advisorEmail) {
      throw new Error('President or advisor email is missing');
    }

    const pool = await sql.connect(sqlConfig);

    const result = await pool.request()
      .input('email1', sql.VarChar, presidentEmail.toLowerCase())
      .input('email2', sql.VarChar, advisorEmail.toLowerCase())
      .query(`
        SELECT userID, LOWER(email) AS email, firstName, lastName FROM [user]
        WHERE LOWER(email) = @email1 OR LOWER(email) = @email2
      `);

    const rows = result.recordset;
    if (rows.length < 2) throw new Error("One or both emails not found");

    let president = null, advisor = null;

    for (const row of rows) {
      if (row.email === presidentEmail.toLowerCase()) {
        president = { id: row.userID, name: `${row.firstName} ${row.lastName}` };
      } else if (row.email === advisorEmail.toLowerCase()) {
        advisor = { id: row.userID };
      }
    }

    if (!president || !advisor) throw new Error("Missing user roles");

    return {
      presidentID: president.id,
      presidentName: president.name,
      advisorID: advisor.id
    };
  } catch (err) {
    console.error('SQL error:', err);
    throw err;
  }
}

router.get('/events', async (req, res) => {
  try {
    const response = await axios.get('http://PRO290OcelotAPIGateway:8080/eventserviceapi/api/events');
    res.render('event', { events: response.data.events || [] });
  } catch (err) {
    console.error('Error fetching events:', err.message);
    res.render('event', { events: [] });
  }
});

router.get('/logout', (req, res) => {
  res.clearCookie('token');
  return res.redirect('/');
});


module.exports = router;
