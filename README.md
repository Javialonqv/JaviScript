# JaviScript Language

Now yes, the **DEFINITIVE** version of my OWN programming language is here (work in progress), with some changes in the syntax and more functions!

## Syntax
This language in an interpretated one, readed line by line, where a line represents a single command to execute.

They're 2 types of commands in the language, the `Built-In commands` and the `Functions`.
- `Built-In commands`: These commands are written, the **command itself**, and **space** and the **parameters** splited by **commas**.\
  Syntax: `<command> <parm1>, <parm2>` or `<command> <parm1>,<parm2>`.
- `Functions`: These are the functions of the language, you can use the integrated ones or a custom one. They are written, the **function name**, **parenthesis** and the parameters inside splited by **commas**.\
  Syntax: `<function>(<parm1>, <parm2>)`.
  
Use a `#` at the start of the line to write comments in the code.

## Buil-In Commands
The built-in commands are the following ones:
`import <name>` To import an specific library.
`var <name> <value>` To create a variable with the specified name and value.
`Func <name>()` To start creating a **custom function** with the specified name.
`return <value>` To return a value if it's inside of a custom function.
`EndFunc` To stop the block of code that represents the custom function.
`If <conditions>` To create an If block with the specified conditions.
`Else` To create a new block of code so, if the previous if it's NOT true, then this code will be executed.
`ElseIf <conditon>` To create a new block of code so, if the previous if it's NOT true AND the next conditions are TRUE, execute this code.
`EndIf` To end a block of code that represents an If one.

## Libraries
The libraries are files with functions included in the interpreter of the language, those functions can be executed ONLY if you import then using the `import` command.
The syntaxis is `import <libraryNme>`.

The parameters between **[]** are optional.

### Main Library
This library is automatically loaded when the interpreter start, this is the only library you don't need to import manually.
The functions are the following ones:
`print(value)` To print the specified value in the console.
`printl(value)` To print the specified value in the console AND prints a new line.
`exit([<code>])` Exits from the application with the specified exit code, 0 if no exit code if specified.
