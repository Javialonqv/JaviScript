using System;
using System.Collections.Generic;
using System.Data;

namespace Interpreter
{
    // <summary>
    // Provides functionality to evaluate mathematical expressions contained within a string.
    // This class can handle both numerical calculations and string concatenation involving numbers.
    // </summary>
    public class ExpressionEvaluator
    {
        // <summary>
        // Evaluates a mathematical expression or concatenation provided as a string.
        //
        // Parameters:
        // - expression: A string representing the mathematical expression to evaluate.
        //
        // Returns:
        // - A string that is the result of the evaluated expression. If the expression is purely numeric,
        //   it returns the result as a string. If it involves concatenation, it returns the concatenated result.
        //
        // Exceptions:
        // - Throws an ArgumentException if the input expression is null or empty.
        // </summary>
        public string Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException("Expression cannot be null or empty.");
            }

            // Check if the expression contains concatenation (e.g., "Hello" + " World")
            if (expression.Contains("+"))
            {
                return HandleConcatenation(expression);
            }
            else
            {
                return EvaluateMathExpression(expression).ToString();
            }
        }

        // <summary>
        // Handles string concatenation in the provided expression.
        //
        // Parameters:
        // - expression: A string representing the concatenation expression to evaluate.
        //
        // Returns:
        // - A string that is the result of the concatenation.
        // </summary>
        private string HandleConcatenation(string expression)
        {
            string[] parts = expression.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            string result = string.Empty;

            foreach (var part in parts)
            {
                result += part.Trim().Trim('\"'); // Trim whitespace and concatenate.
            }

            return result;
        }

        // <summary>
        // Evaluates a mathematical expression using DataTable.Compute method.
        //
        // Parameters:
        // - expression: A string representing the mathematical expression to evaluate.
        //
        // Returns:
        // - An object that is the result of the evaluated mathematical expression.
        // </summary>
        private object EvaluateMathExpression(string expression)
        {
            DataTable table = new DataTable();
            return table.Compute(expression, string.Empty);
        }
    }
}