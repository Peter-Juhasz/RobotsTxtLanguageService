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
