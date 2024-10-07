## Notes

This solution is provided as a template.  It contains just enough code to load
the input CSV files.  We are hoping that this template would save you some time.

Feel free to change anything.  You may also opt not to use this template
solution.

Here's an example of how to run the program, `patch_diff`, the buid output of the
project `PatchDiff`.  From the directory where this README file is located:

```
$ ./PatchDiff/bin/Debug/net8.0/patch_diff --help
A patch file diffing tool (LightKeeper backend developer code test)

Usage:
patch_diff (-h | --help)
patch_diff <patch_left> <patch_right>

Options:
  -h --help                  Show this screen.
  <patch_left>               Path to a patch file.
  <patch_right>              Path to another patch file.

$ ./PatchDiff/bin/Debug/net8.0/patch_diff Patch0.csv Patch1.csv
Header: BeginDate
Header: EndDate
Header: Issuer
.
.
.
```

The program, at its unmodifed state, simply prints out the patch file contents.
This is of course not what you are asked to do...

Good luck.


```
Testing Notes
=============

Included Test Files
====================
Patch0.csv
Patch1.csv
Patch2.csv
Patch3.csv
Patch4.csv
Patch5.csv
Patch6.csv
Patch7.csv
Patch8.csv
Patch9.csv
Patch10.csv
Patch11.csv
Patch12.csv


*	Key Column Name in both the files must match. The first non-BeginDate/non-EndDate column is called the key column.

	Example: Happy Path
	===================

	Patch5.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector
	,,JBL,USA,,,Computers

	Patch6.csv has the following data.

	BeginDate,Issuer,EndDate,Country,Conviction,Industry,Sector
	,JBL,,USA,,,Computers

	Based on this data Issuer is the Key Column Name.

	Command:  ./patch_diff.exe "Patch5.csv", "Patch6.csv"
	=======

	Example: Not so Happy Path
	==========================

	Patch7.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector
	,,JBL,USA,,,Computers

	Patch8.csv has the following data.

	BeginDate,Country,EndDate,Issuer,Conviction,Industry,Sector
	,USA,,JBL,,,Computers

	Based on this data Issuer is the Key Column Name for Patch7.csv. But, Country is the Key column for Patch8.csv.

	Command:  ./patch_diff.exe "Patch7.csv", "Patch8.csv"
	=======

*   Detect for duplicate keys.

	Example: Happy Path
	===================

	Patch0.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector
	,,JBL,USA,,,Computers
	,,PIPR,FRA,,,Consumer Discretionary
	,,FR,USA,,,

	Patch1.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector
	,,JBL,USA,,,Computers
	,,PIPR,FRA,,,Consumer Discretionary
	,,FR,USA,,,

	Based on this data, there are no duplicate keys.

	Command:  ./patch_diff.exe "Patch0.csv", "Patch1.csv"
	========

	Example: Not so Happy Path
	==========================

	Patch0.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector
	,,JBL,USA,,,Computers
	,,PIPR,FRA,,,Consumer Discretionary
	,,FR,USA,,,

	Patch2.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector,Analyst
	,,JBL,USA,,,Computers,
	,,PIPR,FRA,,,Consumer Discretionary,
	,,FR,USA,,,,
	20240201,,FR,FRA,Low,,,Bob
	,,PIPR,USA,High,,,

	Based on this data, there are duplicate keys in Patch2.csv.

	Command:  ./patch_diff.exe "Patch0.csv", "Patch2.csv"
	========

*	Check for Empty Keys

	Example: Happy Path
	===================

	Patch4.csv has the following data.

	BeginDate,EndDate,Issuer,Sector,Industry,Country,Conviction,Analyst
	,,JBL,Computers,,USA,,
	,,PIPR,Consumer Discretionary,,USA,High,
	,,FR,,,USA,,
	20240201,,FR1,,,FRA,Low,Sally
	20240301,,IBM,,,,Low,

	Based on this data, there are empty keys.

	Command:  ./patch_diff.exe "Patch4.csv", "Patch9.csv"
	========

	Example: Not so Happy Path
	==========================

	Patch3.csv has the following data.

	BeginDate,EndDate,Issuer,Sector,Industry,Country,Conviction,Analyst
	,,JBL,Computers,,USA,,
	,,,Consumer Discretionary,,FRA,,

	Based on this data, there are empty keys.

	Command:  ./patch_diff.exe "Patch3.csv", "Patch3.csv"
	========
	
*	Column Removed from Old Patch file 

	Example: 
	========

	Patch0.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector
	,,JBL,USA,,,Computers
	,,PIPR,FRA,,,Consumer Discretionary
	,,FR,USA,,,

	Patch10.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sectors,test
	,,JBL,USA,,,Computers,Main test
	,,PIPR,FRA,,,Consumer Discretionary,
	,,FR,USA,,,,

	Based on this data, column sector removed from old patch file.

	Command:  ./patch_diff.exe "Patch0.csv", "Patch10.csv"
	========

*	New Column Added to New Patch file.

	Patch0.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector
	,,JBL,USA,,,Computers
	,,PIPR,FRA,,,Consumer Discretionary
	,,FR,USA,,,

	Patch11.csv has the following data.

	BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector,test
	,,JBL,USA,,,Computers,Main test
	,,PIPR,FRA,,,Consumer Discretionary,
	,,FR,USA,,,,

	Based on this data, column sector removed from old patch file.

	Command:  ./patch_diff.exe "Patch0.csv", "Patch11.csv"
	========

*	Detect changes

	Command:  ./patch_diff.exe "Patch0.csv", "Patch12.csv"
	========

*	Commands
	========
	./patch_diff.exe "Patch5.csv", "Patch6.csv"
	./patch_diff.exe "Patch7.csv", "Patch8.csv"
	./patch_diff.exe "Patch0.csv", "Patch1.csv"
	./patch_diff.exe "Patch0.csv", "Patch2.csv"
	./patch_diff.exe "Patch4.csv", "Patch9.csv"
	./patch_diff.exe "Patch3.csv", "Patch3.csv"
	./patch_diff.exe "Patch0.csv", "Patch10.csv"
	./patch_diff.exe "Patch0.csv", "Patch11.csv"
	./patch_diff.exe "Patch0.csv", "Patch12.csv"


```