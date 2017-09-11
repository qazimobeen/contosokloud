const builder = require("botbuilder");
var teams = require("botbuilder-teams");
const faker = require('faker');
const utils = require('../utils/utils.js');

///////////////////////////////////////////////////////
//	Local Variables
///////////////////////////////////////////////////////
var connector; // Connector from botbuilder sdk
var bot; //Bot from botbuilder sdk
var server; //Dictionary that maps task IDs to messages that have already been sent.


// example for compose extension
var searchHandler = function (event, query, callback) {
    // parameters should be identical to manifest
    if (query.parameters[0].name != "search") {
        return callback(new Error("Parameter mismatch in manifest"), null, 500);
    }

    try {
		var results = generateResults();
		callback(null, results, 200 );
    }
    catch (e) {
        callback(e, null, 500);
    }
};



///////////////////////////////////////////////////////
//	Helpers and other methods
///////////////////////////////////////////////////////
// This generates an array of random results
function generateResults(){
	var results = {
		composeExtension:{
			attachmentLayout: 'list',
			type: 'result',			
		}
	}

	var attachments = [];
	for(var i = 0; i < 5; i++){
		attachments.push(generateResultAttachment());
	}

	results.composeExtension.attachments = attachments;
	return results;
}

// This generates a single random thumbnail
function generateThumbnail(){
	return {
		contentType: 'application/vnd.microsoft.card.thumbnail',
		content: {
			title: utils.getTitle(),
			subtitle: `Assigned to ${utils.getName()}`,
			text: faker.fake('{{lorem.sentence}}'),
			images: [
				{
					url: `${process.env.BASE_URI}/static/img/image${Math.floor(Math.random() * 9) + 1}.png`
				}
			]
		}
	}
}

// This generates a single random result
function generateResultAttachment(){
	var attachment = generateThumbnail();
	attachment.preview = generateThumbnail();
	return attachment;
}

///////////////////////////////////////////////////////
//	Exports
///////////////////////////////////////////////////////
module.exports.init = function(server, connector, bot) {
	this.server = server;
	this.connector = connector;
	this.bot = bot;
	this.connector.onQuery('searchCmd', searchHandler);
	return this;
}





