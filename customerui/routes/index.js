const express = require('express');
const axios = require('axios');
const crypto = require('crypto'); // ✅ Move this to the top for clarity
const router = express.Router();

// ✅ Render the login page at the root
router.get('/', (req, res) => {
  res.render('login');
});

// ✅ Render the signup page
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
      // ✅ Redirect to login page after success
      return res.redirect('/'); // or '/login' if you have a /login route
    } else {
      // Render signup page again with error
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
    const response = await axios.get('http://PRO290UserServiceAPI:8080/api/users');
    const users = response.data.Users;

    if (!users || !Array.isArray(users)) {
      return res.render('login', { error: 'User list unavailable.' });
    }

    const user = users.find(u => u.email === email);
    if (!user) {
      return res.render('login', { error: 'User not found.' });
    }

    const hashedPassword = crypto.createHash('sha256').update(password).digest();

    if (!user.password || !user.password.data) {
      return res.render('login', { error: 'Invalid stored password format.' });
    }

    const storedPassword = Buffer.from(user.password.data);
    if (Buffer.compare(storedPassword, hashedPassword) !== 0) {
      return res.render('login', { error: 'Incorrect password.' });
    }

    return res.redirect('/club');
  } catch (err) {
    console.error(err.message);
    return res.status(500).render('login', { error: 'Login failed. Try again.' });
  }
});

// ✅ Handle GET /club to render the club page
router.get('/club', (req, res) => {
  // For now, use dummy data — you can fetch real data from ClubService later
  res.render('club', {
    clubName: 'Test Club',
    clubDeclaration: 'To build cool stuff',
    presidentName: 'Jane Doe',
    members: [
      { firstName: 'Alice', lastName: 'Smith' },
      { firstName: 'Bob', lastName: 'Johnson' }
    ]
  });
});


module.exports = router;
