const jwt = require('jsonwebtoken');

function authorizeRoles(allowedRoles = []) {
  return (req, res, next) => {
    const token = req.cookies.token;

    console.log(`token ${token}`)

    if (!token) {
      return res.status(401).send('Unauthorized: No token provided.');
    }

    try {
      const decoded = jwt.verify(token, 'SuperSecretPasswordWithASuperSecretSecretThatNoOneWillEverFind');

      console.log(`decoded JWT`, JSON.stringify(decoded, null, 2))

      const role = decoded?.role?.toLowerCase() || decoded?.Role?.toLowerCase() || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]?.toLowerCase();

      console.log(`role ${role}`)

      if (!allowedRoles.includes(role)) {
        return res.status(403).send('Forbidden: Insufficient permissions.');
      }

      req.user = decoded; // Attach decoded user to request
      next();
    } catch (err) {
      return res.status(400).send('Invalid token.');
    }
  };
}

module.exports = authorizeRoles;
