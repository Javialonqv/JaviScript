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

## Built-In Commands
The built-in commands are the following ones:\
`import <name>` To import an specific library.\
`var <name> <value>` To create a variable with the specified name and value.\
`Func <name>()` To start creating a **custom function** with the specified name.\
`return <value>` To return a value if it's inside of a custom function.\
`EndFunc` To stop the block of code that represents the custom function.\
`If <conditions>` To create an If block with the specified conditions.\
`Else` To create a new block of code so, if the previous if it's NOT true, then this code will be executed.\
`ElseIf <conditon>` To create a new block of code so, if the previous if it's NOT true AND the next conditions are TRUE, execute this code.\
`EndIf` To end a block of code that represents an If one.

### Variables
The variables are defined with `var <name> <value>`. The two paramethers are required and the value type also can change.\
One you define them, you can't define any variable of **function** with the same name, or it wull throw an error.\
The default values are:\
`null` Put `null`, `Null`, `undefined` or `Undefined`.\
`string` Put the value between quotes or, in some cases without them, but we recommend WITH quotes.\
`int` Put any valid number in the value field.\
`float` Put any valid number with an `f` at the end.\
`bool` Put `true`, `false` OR a bool operator, such as `1 == 1`.

The variables also have functions in them, such as:
`getType()` To get the current type of the variable.

## Libraries
The libraries are files with functions included in the interpreter of the language, those functions can be executed ONLY if you import then using the `import` command.

The parameters between **[]** are optional.

### Main Library
This library is automatically loaded when the interpreter start, this is the only library you don't need to import manually.\
The functions are the following ones:\
`print(value)` To print the specified value in the console.\
`printl(value)` To print the specified value in the console AND prints a new line.\
`exit([<code>])` Exits from the application with the specified exit code, 0 if no exit code if specified.
### Console Library
This library has utilities to interact with the console.\
`pause() or pause(text)` Pauses the code executing and wait for a key from the user.\
`input()` Returns the input of the user in a string.\
`key()` Returns the key pressed by the user.\
`clear()` Clears the console.\
`fgColor(color)` Changes the foreground color of the console.\
`bgColor(color)` Changes the background color of the console.\
### Convert Library
This library has utilities to convert values from a type to another one.\
The functions are the following ones:\
`int(value)` Returns the specified value to an int value.\
`tryInt(value)` Returns true if the specified value can be converted to an int value.\
`str(value)` Returns the specified value to an string value.\
`tryStr(value)` Returns true if the specified value can be converted to an string value.\
`float(value)` Returns the specified value to a float value.\
`tryFloat(value)` Returns true if the specified value can be converted to a float value.
### Internal Library
This library has utilities to execute code or certain operations at internal level.\
The functions are the following ones:\
`execute(text)` Executes the specified command in **JaviScript** language.\
`call(text)` Executes the specified **C#** code.\
`getType(variable)` Returns the type of the specified variable.
