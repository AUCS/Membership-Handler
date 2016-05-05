using MembershipHandler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembershipHandler.Filters
{
    public class StudentIdAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string studentId = ((Member)validationContext.ObjectInstance).StudentId;
            if (studentId.StartsWith("a", StringComparison.InvariantCultureIgnoreCase))
            {
                if (studentId.Length == "a1234567".Length)
                {
                    string number = studentId.Substring(1, studentId.Length - 2);
                    int x;
                    if (int.TryParse(number, out x))
                    {
                        return ValidationResult.Success;
                    }
                }
            }
            return new ValidationResult("Invalid Student Id");
        }
    }
}