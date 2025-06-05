const jwt = require('jsonwebtoken');

function authorizeRoles(allowedRoles = []) {
  return (req, res, next) => {
    const token = req.cookies.token;

    console.log(`token ${token}`)

    if (!token) {
      return res.status(401).render('login', { error: 'Missing token. Please log in.' });
    }

    try {
      const decoded = jwt.verify(token, process.env.JWT_SECRET);

      const role = decoded?.role?.toLowerCase()
                || decoded?.Role?.toLowerCase()
                || decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']?.toLowerCase();

      console.log(`role ${role}`)

      if (!allowedRoles.includes(role)) {
        return res.status(403).render('login', { error: 'You do not have permission to access this page.' });
      }

      req.user = decoded; // attach user info to request
      next();
    } catch (err) {
      console.error('JWT verification error:', err.message);
      return res.status(403).render('login', { error: 'Invalid or expired token.' });
    }
  };
}

module.exports = authorizeRoles;
