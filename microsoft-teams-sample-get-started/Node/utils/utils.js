const faker = require('faker');

// Creates a random task
module.exports.createTask = function (title) {
	return {
		'title': (title) ? title : getTitle(),
		'description': faker.fake("{{lorem.paragraph}}"),
		'assigned': getName()
	}
}

// Generates random names
const names = ['Evangelina Gallagher', 'Jess Lamontagne', 'Darlene Solis', 'Linda Riley', 'Simone Suarez', 'Alfonso Troy', 'Gabriel Hendon'];
function getName() { return names[Math.floor(Math.random() * names.length)]}
module.exports.getName = getName; 

// Generates random task titles
const titles = ['Create new tenant', 'Add new team members', 'Hire two new developers', 'Interview design candidates', 'Set up the project', 'Decide on project tools', 'Assign new tasks', 'Generate new leads', 'Meet with clients', 'Meet with the press', 'Sleep'];
function getTitle() { return titles[Math.floor(Math.random() * titles.length)]}
module.exports.getTitle = getTitle;