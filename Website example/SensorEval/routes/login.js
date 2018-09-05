var express = require('express');
var router = express.Router();

function renderLoginPage(req, res) {
	res.render('login', {
		title: 'Sensor Evaluation'
	});
}

router.get('/',renderLoginPage);

router.post('/', function(req, res) {
});

module.exports = router;
