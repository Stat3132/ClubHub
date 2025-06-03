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
    const response = await axios.post('http://PRO290UserServiceAPI:8080/api/users/login', {
      Email: email,
      Password: password
    });

    const data = response.data;

    if (!data.success || !data.token) {
      return res.render('login', { error: data.message || 'Invalid login.' });
    }

    console.log('JWT Token:', data.token);
    return res.redirect('/home');
  } catch (err) {
    console.error('Login error:', err.response?.data || err.message);
    return res.status(401).render('login', { error: 'Login failed. Check your credentials.' });
  }
});

router.get('/home', async (req, res) => {
  try {
    const response = await axios.get('http://PRO290ClubServiceAPI:8080/api/clubs');
    const clubs = response.data.clubs || [];
    res.render('home', { clubs });
  } catch (err) {
    console.error('Error fetching clubs:', err.message);
    res.render('home', { clubs: [] });
  }
});

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
      if (row.email === presidentEmail) {
        president = { id: row.userID, name: `${row.firstName} ${row.lastName}` };
      } else if (row.email === advisorEmail) {
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
      presidentName,
      presidentID,
      advisorID
    });

    if (response.data.success) {
      return res.redirect('/home');
    } else {
      return res.render('createclub', { error: response.data.message });
    }

  } catch (err) {
    console.error('Error creating club:', err.message);
    return res.status(500).render('createclub', { error: 'Failed to create club. ' + err.message });
  }
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
