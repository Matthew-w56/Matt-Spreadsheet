
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
		and keeps track of what cells have something in them.  This Spreadsheet class can be
		saved to a file, and then loaded again later.

	2. Future Functionality
		Eventually, the Spreadsheet will be given a GUI and will be interact-able with a user that way.

# Examples of Good Software Practice

	Example 1: Tests
	I created more tests than I usually do before starting to code.  This helped me have a guideline as
	I coded and showed me what is next.

	Example 2: Separation of concerns
	As I refactored my A4 code, I make sure that methods that could be simplified were, and I broke on method
	specifically, the Spreadsheet.StoreCellContents method (helper method) into parts for which data type it was.

	Example 3: Regression Testing
	After I finished the draft of my code, I went through and ran all my previous tests and made sure that they
	were working so I know I had a solid foundation to go off of.

# Time Expenditures:
	(Future assignments' predicted time TBD)

	Assignment				Predicted	Actual		Notes
	1 - Formula Evaluator	5			2:30		Last half hour spend documenting
	2 - Dependency Graph	4			3:12		Fought with Stress test for 1.5 hours
	3 - Formula Class		4			4:45		Spent 30 minutes adjusting Visual Studio settings
	
	Better Time Breakout (Should have done this earlier)
	Assignment				Predicted	Coding		Debugging	Testing		Documenting	Total
	4 - Spreadsheet Class	5			0:45		0:03		1:18		0:35		2:41
	5 - Spreadsheet Saving	7			1:40		1:45		0:31		0:25		4:21