# Robots.txt Language Service
Visual Studio language service for robots.txt.

Features:
 - Syntax Highlighting
 - Outlining
 - Completion
 - Diagnostics, code fixes and refactorings
 - Reference highlighting
 - Quick info, documentation
 - Automatic formatting (indentation and white space on ':')

![robots](https://cloud.githubusercontent.com/assets/9047283/8263672/734138c4-16dd-11e5-8113-c9a10decc25c.png)

Diagnostics:
 - Syntax errors
 - Schema validation (order of fields, unique fields, values, ...)
 - Information disclosure
 
Code Fixes:
 - Insert missing field name-value delimiter
 - Separate records by a blank line
 - Move User-agent line to the top of the record
 - Change '*' to directory root
 - Remove line exposing a private resource
