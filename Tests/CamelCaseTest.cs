using System;
using System.Collections.Generic;
using DocSaw;
using Xunit;

namespace Tests
{
    public class CamelCaseTest
    {
        [Theory]
        [MemberData(nameof(CorrectTitles))]
        public void CorrectTitle(string title)
        {
            string invalidStart;
            var result = TitleCasing.IsCamelCase(title, out invalidStart);
            Assert.True(result, $"Title '{title}' should be recognized as valid camel case (invalid: '{invalidStart}')");
        }

        [Theory]
        [MemberData(nameof(IncorrectTitles))]
        public void IncorrectTitle(string title)
        {
            var result = TitleCasing.IsCamelCase(title);
            Assert.False(result, $"Title '{title}' should be recognized as invalid camel case");
        }

        public static IEnumerable<object[]> CorrectTitles()
        {
            Func<string, object[]> tc = s => new object[] {s};

            yield return tc("A");
            yield return tc("Single");
            yield return tc("Two Words");
            yield return tc("Drift Due to Initial Conditions");
            yield return tc("Moving the Chicken");
            yield return tc("Chicken Despite Chicken");
            yield return tc("Chicken and Chicken");
            yield return tc("Chicken after Chicken");
            yield return tc("Customisation - Custom OBC Code, APIs, Interacting with Subsystems");
            yield return tc("QM PLD Integration with a Satellite, Assembly Verification");
            yield return tc("Satellite Is Transported for Integration with a P-POD, Integration with a P-POD");
            yield return tc("4.2 Chickencraft Heritage and Specification");
            yield return tc("Chicken Chicken Chicken (CCC)");
            yield return tc("Chicken Chicken at This Chicken:");
            yield return tc("   Chicken Chicken at This Chicken");
            yield return tc("");

            yield return tc("600 km");
            yield return tc("UHF / VHF"); // will do for now
            // yield return tc("UHF/VHF"); // would be better

            yield return tc("Appendix A - Something Here");
            yield return tc("Appendix A");
            yield return tc("Analysis of the Results - Appendix A");

        }

        public static IEnumerable<object[]> IncorrectTitles()
        {
            Func<string, object[]> tc = s => new object[] {s};

            yield return tc("single");
            yield return tc("two Words");
            yield return tc("Two words");
            yield return tc("Chicken After Chicken");
            yield return tc("Simplified Approaches (the \x201cDose-depth\x201d Curve)");
            yield return tc("Title with \"Quotation \" Mark");
            yield return tc("Title with &");
            yield return tc("Title with |");
            yield return tc("Random & Acoustic Chickens");
            yield return tc("Detailed 1-D, 2-D or Full 3-D Chicken Transport Calculations (Monte-Carlo & Finite Difference Models) (TODO)");
            yield return tc("Handling and Transportation Loads - Quasi-static G-loads | Summary");
            yield return tc("Chickement, Chickenbling + Cochickening, Chickenables Released");
            yield return tc("- A-B Interface"); // non printable character
            yield return tc("- Chicken");
            //yield return tc("UHF / VHF"); // will do for now
        }
    }
}
