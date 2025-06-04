const jwt = require('jsonwebtoken');

function authorizeRoles(allowedRoles = []) {
  return (req, res, next) => {
    const token = req.cookies.token;

    if (!token) {
      return res.status(401).send('Unauthorized: No token provided.');
    }

    try {
      const decoded = jwt.verify(token, 'SuperSecretPasswordWithASuperSecretSecretThatNoOneWillEverFind');
      const role = decoded?.role?.toLowerCase() || decoded?.Role?.toLowerCase();

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
