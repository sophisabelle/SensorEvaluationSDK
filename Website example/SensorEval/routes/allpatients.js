var express = require('express');
var router = express.Router();

/* GET page. */

var requests = require('../includes/functions_db');

function getPatients(req, res, next) {
	var sql = "SELECT * FROM Patients ORDER BY Name DESC";
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			req.patients = rows;
			next();
		}
	});
}

function renderPatientsPage(req, res) {
	res.render('allpatients', {
		title: 'Sensor Evaluation',
		patients: req.patients
	});
}

router.get('/', getPatients, renderPatientsPage);

module.exports = router;