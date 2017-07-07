using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q_Zone.Models.Utility;

namespace Q_Zone.Tests.Models.Utility
{
    [TestClass()]
    public class ValidFormatTests
    {
        [TestMethod()]
        public void IsEmailFormatTest() {
            // sample valid email format for test
            string[] validEmailFromatList =
            {
                "prettyandsimple@example.com",
                "very.common@example.com",
                "disposable.style.email.with+symbol@example.com",
                "other.email-with-dash@example.com",
                "x@example.com",
                "example@s.solutions"
            };

            // sample invalid email format for test
            string[] invalidEmailFormatList =
            {
                "Abc.example.com",
                "A@b@c@example.com",
                "a\"b(c)d,e:f;g<h>i[j\\k]l@example.com",
                "just\"not\"right@example.com",
                "this is\"not\\allowed@example.com",
                "this\\ still\\\"not\\\\allowed@example.com",
                "john..doe@example.com",
                "john.doe@example..com",
                " example@test.com",
                "example@test.com "
            };

            // check valid email format
            foreach (string email in validEmailFromatList) {
                Assert.IsTrue(ValidFormat.IsEmailFormat(email),
                    "Valid email format not detected for: \"" + email + "\".");
            }

            // check invalid email format
            foreach (string email in invalidEmailFormatList) {
                Assert.IsFalse(ValidFormat.IsEmailFormat(email),
                    "Invalid email format not detected for: \"" + email + "\".");
            }
        }
    }
}