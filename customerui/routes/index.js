const express = require('express');
const axios = require('axios');
const crypto = require('crypto'); 
const router = express.Router();



// login page
router.get('/', (req, res) => {
  res.render('login');
});

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

    if (response.data.Success) {
      
      return res.redirect('/'); 
    } else {
      
      return res.render('signup', { error: response.data.Message });
    }
  } catch (err) {
    console.error('Signup error:', err.message);
    return res.status(500).render('signup', { error: 'Signup failed. Try again.' });
  }
});

router.post('/login', async (req, res) => {
  const { email, password } = req.body;

  try {
    // Step 1: Authenticate with UserService
    const response = await axios.post('http://PRO290UserServiceAPI:8080/api/users/login', {
      Email: email,
      Password: password
    });

    const data = response.data;

    if (!data.success || !data.token) {
      return res.render('login', { error: data.message || 'Invalid login.' });
    }

    // Step 2: Get user role and ID
    const pool = await sql.connect(sqlConfig);
    const result = await pool.request()
      .input('email', sql.VarChar, email.toLowerCase())
      .query(`SELECT role, userID FROM [user] WHERE LOWER(email) = @email`);

    if (result.recordset.length === 0) {
      return res.render('login', { error: 'User not found in system.' });
    }

    const role = result.recordset[0].role.toLowerCase();
    const userID = result.recordset[0].userID;

    console.log(`✅ ${email} logged in as ${role}`);

    // Step 3: Role-based redirect
    if (role === 'admin') {
      return res.redirect('/manage'); // Admin sees all users
    }

    if (role === 'advisor') {
      const clubResult = await pool.request()
        .input('userID', sql.UniqueIdentifier, userID)
        .query(`
          SELECT TOP 1 clubID FROM club
          WHERE advisorID = @userID
        `);

      if (clubResult.recordset.length > 0) {
        const clubID = clubResult.recordset[0].clubID;
        return res.redirect(`/manage/${clubID}`);
      }

      // If no club found
      return res.redirect('/home');
    }

    // Students/presidents/others
    return res.redirect('/home');

  } catch (err) {
    console.error('Login error:', err.response?.data || err.message);
    return res.status(401).render('login', { error: 'Login failed. Check your credentials.' });
  }
});

router.get('/home', async (req, res) => {
  try {
    const response = await axios.get('http://PRO290ClubServiceAPI:8080/api/clubs');
    const clubs = response.data;

    res.render('home', { clubs });
  } catch (err) {
    console.error('Error fetching clubs:', err.message);
    res.render('home', { clubs: [] }); 
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
router.get('/approveClubRequest', (req, res) => {
  res.render('approveClubRequest');
});

router.get('/addMember', (req, res) => {
  res.render('addMember');
});

router.get('/removeMember', (req, res) => {
  res.render('removeMember');
});

router.get('/addEvent', (req, res) => {
  res.render('addEvent');
});

// Admin POST routes
router.post('/approveClubRequest', async (req, res) => {
  // Accept club logic here
  res.send('Club request accepted (mock response).');
});

router.post('/addMember', async (req, res) => {
  // Add user to club logic here
  res.send('User added to club (mock response).');
});

router.post('/removeMember', async (req, res) => {
  // Remove user from club logic here
  res.send('User removed from club (mock response).');
});

router.post('/addEvent', async (req, res) => {
  // Add event to club logic here
  res.send('Event added (mock response).');
});



// END OF ADMIN ROUTES /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


const sql = require('mssql');

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


router.get('/create-club', (req, res) => {
  res.render('createclub');
});

router.post('/create-club', async (req, res) => {
  const { clubName, clubDeclaration, presidentEmail, advisorEmail } = req.body;

  try {
    const { presidentID, presidentName, advisorID } = await getClubUserDetails(presidentEmail, advisorEmail);

    const response = await axios.post('http://PRO290ClubServiceAPI:8080/api/clubs', {
      clubName,
      clubDeclaration,
      clubPresidentName: presidentName, 
      clubPresidentID: presidentID,         
      advisorID
    });

    if (response.status === 201) {
      return res.render('createclub', {
        success: true,
        clubName
      });
    } else {
      return res.render('createclub', { error: response.data.message });
    }

  } catch (err) {
    console.error('Error creating club:', err.message);
    return res.status(500).render('createclub', {
      error: 'Failed to create club. ' + err.message
    });
  }
});


router.get('/contact', (req, res) => {
  res.render('contact');
});

router.post('/contact', async (req, res) => {
  const { senderName, senderEmail, recipientEmail, message } = req.body;
  try {
    await axios.post('http://PRO290MessageServiceAPI:8080/api/messages', {
      senderName,
      senderEmail,
      recipientEmail,
      message
    });
    res.redirect('/home');
  } catch (err) {
    console.error('Error sending message:', err.message);
    res.status(500).send('Failed to send message.');
  }
});

router.get('/events', async (req, res) => {
  try {
    const response = await axios.get('http://PRO290EventServiceAPI:8080/api/events');
    res.render('event', { events: response.data.events || [] });
  } catch (err) {
    console.error('Error fetching events:', err.message);
    res.render('event', { events: [] });
  }
});




module.exports = router;
