
Author:			Matthew Williams
Partner:		None
Start Date:		24-Feb-2023
Course:			CS 3500 - Software Practice - University of Utah
Github ID:		matthew-w56
Repo:			https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
Finish Date:	02-MAR-2023
Project:		Maui GUI
Copyright:		CS 3500 and Matthew Williams - This work may not be copied for use in Academic Coursework

Personal Statement: I wrote this code myself and did not take it from any other sources other than those listed below.

# Summary

	This class allows the user to interact with the rest of the Project.  It shows the grid, allows saving and loading,
	and gives in-program help for the user.

# Additional Features and Design Decisions

	The primary additional feature I added was a small list of Functions that you can type in, similar to Excel.  Their use
	is in the form FUNCTION(var1:var2).  This also means that I implemented a Range class that represents a set of names.
	This is done through a new class that extends Formula called EnhancedFormula.  It is very similar, but it can parse and
	evaluate these functions.  The range stores the boundaries of it's extent, and can return a list of all it's members.
	The help menu (accessible through the File menu) has a section on Functions in general, as well as for each Function.
	One detail that I thought worked quite well is that as the EnhancedFormula is being checked for validity upon
	instantiation and it hits a Function, it saves the associated Range indexed by the current count iterator for the loop.
	Then, later during evaluation, the current i is used to find the range again as it is being parsed without having to
	parse the Range twice.

	A few additional features include displaying the current file name up top for any spreadsheet that has a file associated.
	When you save a spreadsheet that already has a file associated, it just saves it to that same location without asking for
	a path.  When saving normally, you just put in a name and it saves it to your desktop.

	I also implemented the feature that when you press enter, it moves you down a cell.

# Partnership

	I didn't work with a partner for this project.

# Comments to Evaluators

	There are 2 known problems with this project.

	First, The box up top that displays the current value of the selected cell will cut off it's content very early.  I specify
	that the width should be way larger (and the box is drawn that large) but it still cuts it off.  I can't solve it, and it is
	a Maui-native problem so I left it as is.  I spent probably 20 minutes attempting to solve this problem.

	Second, it is really slow to do anything if you create a function off to the side that encompasses a large area of cells.  I
	could spend hours and hours attempting to make this more efficient and quick, but the deep cause is that Maui isn't set up for
	this kind of project built in this kind of way anyways, so I left it as is.  I did not spend time on this problem.

	NOTE: I am on a Windows Machine running Windows 10

# Assignment Specific Topics
	
	Nothing for this assignment, as far as I know.

# Consulted Peers
	
	No peers consulted - I spoke with others in the class, just not about/for help on this assignment.

# References

	StackOverflow - Help removing title from Maui window
		URL: https://stackoverflow.com/questions/71806578/maui-how-to-remove-the-title-bar-and-fix-the-window-size
	Microsoft Help - Using Pop-up Windows in Maui
		URL: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups?view=net-maui-7.0
	StackOverflow - Help with extra feature (specifically, looping over letters)
		URL: https://stackoverflow.com/questions/2208688/quickest-way-to-enumerate-the-alphabet
	Microsoft Excel Pages - Brainstorming Functions to add
		URL: https://support.microsoft.com/en-us/office/excel-functions-alphabetical-b3944572-255d-4efb-bb96-c6d90033e188
