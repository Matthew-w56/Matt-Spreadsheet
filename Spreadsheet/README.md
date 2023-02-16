
Author:			Matthew Williams
Partner:		None
Start Date:		03-Feb-2023
Course:			CS 3500 - Software Practice - University of Utah
Github ID:		matthew-w56
Repo:			https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
Finish Date:	03-Feb-2023
Project:		Spreadsheet Class
Copyright:		CS 3500 and Matthew Williams - This work may not be copied for use in Academic Coursework

Personal Statement: I wrote this code myself and did not take it from any other sources other than those listed below.

# Summary

	This project contains a class representing a Spreadsheet in our program.  It also contains
	the abstract class that the Spreadsheet class is implementing.  This class tracks dependencies
	utilizing the DependencyGraph project, and treats Formulas as it should.  Cells can be set with
	contents, and manipulated using the API of the class.

# Comments to Evaluators

	One small functionality that I was unsure if was included in this assignment was this: A cell that
	has already been given a value and is included in the list of cells with non-empty contents being
	set with the value "" should be removed from the list and therefore deleted.  I wasn't aware of
	any instruction to include this feature, but it made sense and so I implemented it in my project.
	You will see a test method that checks the working of this feature in my SpreadsheetTests file.

# Software Practice

	During this project, I utilized more code reuse and documentation practices.

# Assignment Specific Topics
	
	Nothing for this assignment, as far as I know.

# Consulted Peers
	
	No peers consulted - I spoke with others in the class, just not about/for help on this assignment.

# References

	Regex101 - Used to make sure my altered Regex worked before plugging it in
		URL: https://regex101.com/
	Microsoft Documentation - Mainly used for XmlReader and XmlWriter methods.
		URL: Multiple Pages Used