const express = require('express');
const jwt = require('jsonwebtoken');

const app = express();
const SECRET = 'secretkey';

function authenticate(req, res, next) {
  const authHeader = req.headers['authorization'];
  if (!authHeader) return res.sendStatus(401);
  const token = authHeader.split(' ')[1];
  jwt.verify(token, SECRET, (err, user) => {
    if (err) return res.sendStatus(403);
    req.user = user;
    next();
  });
}

app.get('/sales', authenticate, (req, res) => {
  res.json([{ id: 1, item: 'Item', quantity: 2 }]);
});

app.listen(3003, () => console.log('SalesService running on port 3003'));
