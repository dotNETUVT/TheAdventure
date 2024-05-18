using System;
using System.IO;

namespace YourProjectNamespace
{
    public class ErrorHandling
    {
        public string ReadFile(string filePath)
        {
            try
            {
                // Attempt to read the file
                return File.ReadAllText(filePath);
            }
            catch (FileNotFoundException ex)
            {
                // Handle the case where the file is not found
                Console.WriteLine($"File not found: {ex.Message}");
            }
            catch (IOException ex)
            {
                // Handle other IO-related exceptions
                Console.WriteLine($"IO exception: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle unauthorized access exceptions (e.g., permission denied)
                Console.WriteLine($"Unauthorized access: {ex.Message}");
            }
            catch (NotSupportedException ex)
            {
                // Handle operations not supported exceptions
                Console.WriteLine($"Operation not supported: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            // Return null or some default value indicating failure
            return null;
        }
    }

        
        