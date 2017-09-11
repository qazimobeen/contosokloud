const moment = require('moment');
const utils = require('../utils/utils.js');
const files = require('../utils/files.js');

///////////////////////////////////////////////////////
//	Local Variables
///////////////////////////////////////////////////////
var server;  //Restify server

///////////////////////////////////////////////////////
//	Bot and listening
///////////////////////////////////////////////////////
function start_listening() {

	this.server.get('tabs/index', (req, res, next) => {
		files.sendFileOrLogin('./tabs/index.html', req, res, next);
	});

	this.server.get('tabs/about', (req, res, next) => {
		files.sendFile('./tabs/about.html', res);
	});

	this.server.get('tabs/configure', (req, res, next) => {
		files.sendFileOrLogin('./tabs/configure.html', req, res, next);
	});

	this.server.get('api/tasks/team', (req, res, next) => {

		var numdays = (typeof req.params.numdays === 'string') ? parseInt(req.params.numdays) : 5;

		var ret = [];
		for (var i = 0; i < numdays; i ++){
			
			var title = (i == 0) ? 'Today' : (i == 1) ? 'Tomorrow' : moment(new Date()).add(i,'days').format('dddd');

			var day = {
				'title': title,
				'tasks': []
			}

			var num_tasks = Math.floor(Math.random() * (10 - 2 + 1)) + 2;
			for (var j = 0; j < num_tasks; j++){
				day.tasks.push(utils.createTask());
			}

			ret.push(day);
		}

		res.send(ret);
		res.end();

	});

	this.server.get('api/tasks/my', (req, res, next) => {

		var ret = [];

		var num_tasks = Math.floor(Math.random() * (10 - 2 + 1)) + 2;
		for (var j = 0; j < num_tasks; j++){
			ret.push(utils.createTask());
		}

		res.send(ret);
		res.end();

	});
}

module.exports.init = function(server) {
	this.server = server;
	return this;
}

module.exports.start_listening = start_listening;