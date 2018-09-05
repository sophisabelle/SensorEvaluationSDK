// azure database

var config = {
	userName: 'USERNAME@sensorsevaldb',
	password: 'PASSWORD',
	server: 'SERVER.database.windows.net',

	options: {
		database: 'sensordat',
		encrypt: true
	}
};

module.exports = config;
