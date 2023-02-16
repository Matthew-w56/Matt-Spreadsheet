
Author:		Matthew Williams
Partner:	None
Date:		12-Jan-2023
Course:		CS 3500 - Software Practice - University of Utah
Github ID:	matthew-w56
Repo:		https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
Date:		TBD
Solution:	Spreadsheet
Copyright:	CS 3500 and Matthew Williams - This work may not be copied for use in Academic Coursework


# Overview of the Spreadsheet Functionality

	1. Current Functionality
		Currently, the Solution has a Library that takes care of evaluating the formula
		typed in by the user.  It also has a class that will track the dependencies in the
		cells, and will allow things to get evaluated in the right order.
		These are utilized by a Spreadsheet class that tracks dependencies, utilizes formulas,
		and keeps track of what cells have something in them.

	2. Future Functionality
		Eventually, the Solution will be a full, working Spreadsheet program that allows the user to use
		mathematical formulas between cells, input information, and safe and retrieve their files.
		Features that will arrive soon will likely be handling the values (as opposed to contents) of cells,
		and a GUI to interact with the Spreadsheet with.

# Examples of Good Software Practice

	FILL THIS SECTION OUT
	suggestions:
	DRY
Separation of concerns
Well named, commented, short methods that do a specific job and return a specific result.
Code re-use
Testing strategies
Regression Testing (you did have old tests)
Abstraction

# Time Expenditures:
	(Future assignments' predicted time TBD)

	Assignment				Predicted	Actual		Notes
	1 - Formula Evaluator	5			2:30		Last half hour spend documenting
	2 - Dependency Graph	4			3:12		Fought with Stress test for 1.5 hours
	3 - Formula Class		4			4:45		Spent 30 minutes adjusting Visual Studio settings
	
	Better Time Breakout (Should have done this earlier)
	Assignment				Predicted	Coding		Debugging	Testing		Documenting	Total
	4 - Spreadsheet Class	5			0:45		0:03		1:18		0:35		2:41
	5 - Spreadsheet Saving	7			1:40		0:30		0:00		0:00		