using System;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class RemotePathDoubleQuoteTransformationTest
{
    private IRemotePathTransformation _transformation;

    public RemotePathDoubleQuoteTransformationTest()
    {
        SetUp();
    }

    private void SetUp()
    {
        _transformation = new RemotePathDoubleQuoteTransformation();
    }

    /// <summary>
    /// Test cases from triple-slash comments
    /// </summary>
    [Fact]
    public void Mixed()
    {
        Assert.Equal("\"/var/log/auth.log\"", _transformation.Transform("/var/log/auth.log"));
        Assert.Equal("\"/var/mp3/Guns N' Roses\"", _transformation.Transform("/var/mp3/Guns N' Roses"));
        Assert.Equal("\"/var/garbage!/temp\"", _transformation.Transform("/var/garbage!/temp"));
        Assert.Equal("\"/var/would be 'kewl'!, not?\"", _transformation.Transform("/var/would be 'kewl'!, not?"));
        Assert.Equal("\"\"", _transformation.Transform(string.Empty));
        Assert.Equal("\"Hello \\\"World\\\"\"", _transformation.Transform("Hello \"World\""));
    }

    [Fact]
    public void Null()
    {
        const string path = null;

        try
        {
            _transformation.Transform(path);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("path", ex.ParamName);
        }
    }

    [Fact]
    public void Ampersand_Embedded()
    {
        const string path = "You&Me";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"You&Me\"", actual);
    }

    [Fact]
    public void Ampersand_Leading()
    {
        const string path = "&Or";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"&Or\"", actual);
    }

    [Fact]
    public void Ampersand_LeadingAndTrailing()
    {
        const string path = "&Or&";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"&Or&\"", actual);
    }

    [Fact]
    public void Ampersand_Trailing()
    {
        const string path = "And&";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"And&\"", actual);
    }

    [Fact]
    public void Asterisk_Embedded()
    {
        const string path = "Love*Hate";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Love*Hate\"", actual);
    }

    [Fact]
    public void Asterisk_Leading()
    {
        const string path = "*Times";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"*Times\"", actual);
    }

    [Fact]
    public void Asterisk_LeadingAndTrailing()
    {
        const string path = "*WAR*";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"*WAR*\"", actual);
    }

    [Fact]
    public void Asterisk_Trailing()
    {
        const string path = "Censor*";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Censor*\"", actual);
    }

    [Fact]
    public void Backslash_Embedded()
    {
        const string path = "Hello\\World";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Hello\\World\"", actual);
    }

    [Fact]
    public void Backslash_Leading()
    {
        const string path = "\\Hello";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"\\Hello\"", actual);
    }

    [Fact]
    public void Backslash_LeadingAndTrailing()
    {
        const string path = "\\Hello\\";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"\\Hello\\\"", actual);
    }

    [Fact]
    public void Backslash_Trailing()
    {
        const string path = "HelloWorld\\";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"HelloWorld\\\"", actual);
    }

    [Fact]
    public void Backtick_Embedded()
    {
        const string path = "back`tick";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"back`tick\"", actual);
    }

    [Fact]
    public void Backtick_Leading()
    {
        const string path = "`front";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"`front\"", actual);
    }

    [Fact]
    public void Backtick_LeadingAndTrailing()
    {
        const string path = "`FrontAndBack`";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"`FrontAndBack`\"", actual);
    }

    [Fact]
    public void Backtick_Trailing()
    {
        const string path = "back`";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"back`\"", actual);
    }

    [Fact]
    public void Circumflex_Embedded()
    {
        const string path = "You^Me";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"You^Me\"", actual);
    }

    [Fact]
    public void Circumflex_Leading()
    {
        const string path = "^Or";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"^Or\"", actual);
    }

    [Fact]
    public void Circumflex_LeadingAndTrailing()
    {
        const string path = "^Or^";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"^Or^\"", actual);
    }

    [Fact]
    public void Circumflex_Trailing()
    {
        const string path = "And^";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"And^\"", actual);
    }

    [Fact]
    public void CurlyBrackets_Close_Embedded()
    {
        const string path = "Halo}Devine";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Halo}Devine\"", actual);
    }

    [Fact]
    public void CurlyBrackets_Close_Leading()
    {
        const string path = "}Open";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"}Open\"", actual);
    }

    [Fact]
    public void CurlyBrackets_Close_LeadingAndTrailing()
    {
        const string path = "}Closed}";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"}Closed}\"", actual);
    }

    [Fact]
    public void CurlyBrackets_Close_Trailing()
    {
        const string path = "Finish}";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Finish}\"", actual);
    }

    [Fact]
    public void CurlyBrackets_Open_Embedded()
    {
        const string path = "Halo{Devine";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Halo{Devine\"", actual);
    }

    [Fact]
    public void CurlyBrackets_Open_Leading()
    {
        const string path = "{Open";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"{Open\"", actual);
    }

    [Fact]
    public void CurlyBrackets_Open_LeadingAndTrailing()
    {
        const string path = "{Closed{";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"{Closed{\"", actual);
    }

    [Fact]
    public void CurlyBrackets_Open_Trailing()
    {
        const string path = "Finish{";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Finish{\"", actual);
    }

    [Fact]
    public void Dollar_Embedded()
    {
        const string path = "IGiveYouOne$ForYourThoughts";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"IGiveYouOne$ForYourThoughts\"", actual);
    }

    [Fact]
    public void Dollar_Leading()
    {
        const string path = "$Blues";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"$Blues\"", actual);
    }

    [Fact]
    public void Dollar_LeadingAndTrailing()
    {
        const string path = "$SUM$";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"$SUM$\"", actual);
    }

    [Fact]
    public void Dollar_Trailing()
    {
        const string path = "NotCravingForMore$";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"NotCravingForMore$\"", actual);
    }

    [Fact]
    public void DoubleQuote_Embedded()
    {
        const string path = "DoNot\"MeOnThis";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"DoNot\\\"MeOnThis\"", actual);
    }

    [Fact]
    public void DoubleQuote_Leading()
    {
        const string path = "\"OrNotToQuote";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"\\\"OrNotToQuote\"", actual);
    }

    [Fact]
    public void DoubleQuote_Trailing()
    {
        const string path = "Famous\"";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Famous\\\"\"", actual);
    }

    [Fact]
    public void DoubleQuote_LeadingAndTrailing()
    {
        const string path = "\"OrNotTo\"";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"\\\"OrNotTo\\\"\"", actual);
    }

    [Fact]
    public void Equals_Embedded()
    {
        const string path = "You=Me";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"You=Me\"", actual);
    }

    [Fact]
    public void Equals_Leading()
    {
        const string path = "=Or";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"=Or\"", actual);
    }

    [Fact]
    public void Equals_LeadingAndTrailing()
    {
        const string path = "=Or=";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"=Or=\"", actual);
    }

    [Fact]
    public void Equals_Trailing()
    {
        const string path = "And=";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"And=\"", actual);
    }

    [Fact]
    public void ExclamationMark_Embedded_Single()
    {
        const string path = "/var/garbage!/temp";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"/var/garbage!/temp\"", actual);
    }

    [Fact]
    public void ExclamationMark_Embedded_Sequence()
    {
        const string path = "/var/garbage!!/temp";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"/var/garbage!!/temp\"", actual);
    }

    [Fact]
    public void ExclamationMark_Leading()
    {
        const string path = "!Error";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"!Error\"", actual);
    }

    [Fact]
    public void ExclamationMark_LeadingAndTrailing()
    {
        const string path = "!ignore!";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"!ignore!\"", actual);
    }

    [Fact]
    public void ExclamationMark_Trailing()
    {
        const string path = "Done!";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Done!\"", actual);
    }

    [Fact]
    public void GreaterThan_Embedded()
    {
        const string path = "You>Me";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"You>Me\"", actual);
    }

    [Fact]
    public void GreaterThan_Leading()
    {
        const string path = ">Or";

        var actual = _transformation.Transform(path);

        Assert.Equal("\">Or\"", actual);
    }

    [Fact]
    public void GreaterThan_LeadingAndTrailing()
    {
        const string path = ">Or>";

        var actual = _transformation.Transform(path);

        Assert.Equal("\">Or>\"", actual);
    }

    [Fact]
    public void GreaterThan_Trailing()
    {
        const string path = "And>";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"And>\"", actual);
    }

    [Fact]
    public void Hash_Embedded()
    {
        const string path = "Smoke#EveryDay";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Smoke#EveryDay\"", actual);
    }

    [Fact]
    public void Hash_Leading()
    {
        const string path = "#4Ever";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"#4Ever\"", actual);
    }

    [Fact]
    public void Hash_LeadingAndTrailing()
    {
        const string path = "#4Ever#";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"#4Ever#\"", actual);
    }

    [Fact]
    public void Hash_Trailing()
    {
        const string path = "Legalize#";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Legalize#\"", actual);
    }

    [Fact]
    public void LessThan_Embedded()
    {
        const string path = "You<Me";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"You<Me\"", actual);
    }

    [Fact]
    public void LessThan_Leading()
    {
        const string path = "<Or";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"<Or\"", actual);
    }

    [Fact]
    public void LessThan_LeadingAndTrailing()
    {
        const string path = "<Or<";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"<Or<\"", actual);
    }

    [Fact]
    public void LessThan_Trailing()
    {
        const string path = "And<";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"And<\"", actual);
    }

    [Fact]
    public void NewLine_Embedded()
    {
        const string path = "line\nfeed";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"line\nfeed\"", actual);
    }

    [Fact]
    public void NewLine_Leading()
    {
        const string path = "\nFooter";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"\nFooter\"", actual);
    }

    [Fact]
    public void NewLine_LeadingAndTrailing()
    {
        const string path = "\nBanner\n";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"\nBanner\n\"", actual);
    }

    [Fact]
    public void NewLine_Trailing()
    {
        const string path = "Header\n";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Header\n\"", actual);
    }

    [Fact]
    public void Parentheses_Close_Embedded()
    {
        const string path = "Halo)Devine";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Halo)Devine\"", actual);
    }

    [Fact]
    public void Parentheses_Close_Leading()
    {
        const string path = ")Open";

        var actual = _transformation.Transform(path);

        Assert.Equal("\")Open\"", actual);
    }

    [Fact]
    public void Parentheses_Close_LeadingAndTrailing()
    {
        const string path = ")Closed)";

        var actual = _transformation.Transform(path);

        Assert.Equal("\")Closed)\"", actual);
    }

    [Fact]
    public void Parentheses_Close_Trailing()
    {
        const string path = "Finish)";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Finish)\"", actual);
    }

    [Fact]
    public void Parentheses_Open_Embedded()
    {
        const string path = "Halo(Devine";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Halo(Devine\"", actual);
    }

    [Fact]
    public void Parentheses_Open_Leading()
    {
        const string path = "(Open";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"(Open\"", actual);
    }

    [Fact]
    public void Parentheses_Open_LeadingAndTrailing()
    {
        const string path = "(Closed(";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"(Closed(\"", actual);
    }

    [Fact]
    public void Parentheses_Open_Trailing()
    {
        const string path = "Finish(";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Finish(\"", actual);
    }

    [Fact]
    public void Percentage_Embedded()
    {
        const string path = "Ten%OfOneDollarIsTenCent";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Ten%OfOneDollarIsTenCent\"", actual);
    }

    [Fact]
    public void Percentage_Leading()
    {
        const string path = "%MoreOrLess";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"%MoreOrLess\"", actual);
    }

    [Fact]
    public void Percentage_LeadingAndTrailing()
    {
        const string path = "%USERNAME%";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"%USERNAME%\"", actual);
    }

    [Fact]
    public void Percentage_Trailing()
    {
        const string path = "TakeA%";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"TakeA%\"", actual);
    }

    [Fact]
    public void Pipe_Embedded()
    {
        const string path = "You|Me";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"You|Me\"", actual);
    }

    [Fact]
    public void Pipe_Leading()
    {
        const string path = "|Or";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"|Or\"", actual);
    }

    [Fact]
    public void Pipe_LeadingAndTrailing()
    {
        const string path = "|Or|";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"|Or|\"", actual);
    }

    [Fact]
    public void Pipe_Trailing()
    {
        const string path = "And|";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"And|\"", actual);
    }

    [Fact]
    public void QuestionMark_Embedded()
    {
        const string path = "WhatTimeIsIt?SheSaid";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"WhatTimeIsIt?SheSaid\"", actual);
    }

    [Fact]
    public void QuestionMark_Leading()
    {
        const string path = "?Quizz";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"?Quizz\"", actual);
    }

    [Fact]
    public void QuestionMark_LeadingAndTrailing()
    {
        const string path = "?Crazy?";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"?Crazy?\"", actual);
    }

    [Fact]
    public void QuestionMark_Trailing()
    {
        const string path = "WhatTimeIsLove?";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"WhatTimeIsLove?\"", actual);
    }

    [Fact]
    public void Semicolon_Embedded()
    {
        const string path = "Rain;Storm";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Rain;Storm\"", actual);
    }

    [Fact]
    public void Semicolon_Leading()
    {
        const string path = ";List";

        var actual = _transformation.Transform(path);

        Assert.Equal("\";List\"", actual);
    }

    [Fact]
    public void Semicolon_LeadingAndTrailing()
    {
        const string path = ";Trapped;";

        var actual = _transformation.Transform(path);

        Assert.Equal("\";Trapped;\"", actual);
    }

    [Fact]
    public void Semicolon_Trailing()
    {
        const string path = "Time;";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Time;\"", actual);
    }

    [Fact]
    public void SingleQuote_Embedded_Single()
    {
        const string path = "Rain'Storm";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Rain'Storm\"", actual);
    }

    [Fact]
    public void SingleQuote_Embedded_Sequence()
    {
        const string path = "Rain''Storm";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Rain''Storm\"", actual);
    }

    [Fact]
    public void SingleQuote_Leading()
    {
        const string path = "'List";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"'List\"", actual);
    }

    [Fact]
    public void SingleQuote_LeadingAndTrailing()
    {
        const string path = "'Trapped'";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"'Trapped'\"", actual);
    }

    [Fact]
    public void SingleQuote_Trailing()
    {
        const string path = "Time'";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Time'\"", actual);
    }

    [Fact]
    public void SingleQuoteAndExclamationMark_Embedded()
    {
        const string path = "Rain'!Storm";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Rain'!Storm\"", actual);
    }

    [Fact]
    public void SingleQuoteAndExclamationMark_Leading()
    {
        const string path = "'!Rain";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"'!Rain\"", actual);
    }

    [Fact]
    public void SingleQuoteAndExclamationMark_LeadingAndTrailing()
    {
        const string path = "'!Rain'!";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"'!Rain'!\"", actual);
    }

    [Fact]
    public void SingleQuoteAndExclamationMark_Trailing()
    {
        const string path = "Rain'!";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Rain'!\"", actual);
    }

    [Fact]
    public void Space_Embedded()
    {
        const string path = "You Me";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"You Me\"", actual);
    }

    [Fact]
    public void Space_Leading()
    {
        const string path = " Or";

        var actual = _transformation.Transform(path);

        Assert.Equal("\" Or\"", actual);
    }

    [Fact]
    public void Space_LeadingAndTrailing()
    {
        const string path = " Or ";

        var actual = _transformation.Transform(path);

        Assert.Equal("\" Or \"", actual);
    }

    [Fact]
    public void Space_Trailing()
    {
        const string path = "And ";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"And \"", actual);
    }

    [Fact]
    public void SquareBrackets_Close_Embedded()
    {
        const string path = "Halo]Devine";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Halo]Devine\"", actual);
    }

    [Fact]
    public void SquareBrackets_Close_Leading()
    {
        const string path = "]Open";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"]Open\"", actual);
    }

    [Fact]
    public void SquareBrackets_Close_LeadingAndTrailing()
    {
        const string path = "]Closed]";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"]Closed]\"", actual);
    }

    [Fact]
    public void SquareBrackets_Close_Trailing()
    {
        const string path = "Finish]";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Finish]\"", actual);
    }

    [Fact]
    public void SquareBrackets_Open_Embedded()
    {
        const string path = "Halo[Devine";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Halo[Devine\"", actual);
    }

    [Fact]
    public void SquareBrackets_Open_Leading()
    {
        const string path = "[Open";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"[Open\"", actual);
    }

    [Fact]
    public void SquareBrackets_Open_LeadingAndTrailing()
    {
        const string path = "[Closed[";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"[Closed[\"", actual);
    }

    [Fact]
    public void SquareBrackets_Open_Trailing()
    {
        const string path = "Finish[";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Finish[\"", actual);
    }

    [Fact]
    public void Tab_Embedded()
    {
        const string path = "You\tMe";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"You\tMe\"", actual);
    }

    [Fact]
    public void Tab_Leading()
    {
        const string path = "\tOr";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"\tOr\"", actual);
    }

    [Fact]
    public void Tab_LeadingAndTrailing()
    {
        const string path = "\tOr\t";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"\tOr\t\"", actual);
    }

    [Fact]
    public void Tab_Trailing()
    {
        const string path = "And\t";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"And\t\"", actual);
    }

    [Fact]
    public void Tilde_Embedded()
    {
        const string path = "Seven~Nine";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Seven~Nine\"", actual);
    }

    [Fact]
    public void Tilde_Leading()
    {
        const string path = "~Ten";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"~Ten\"", actual);
    }

    [Fact]
    public void Tilde_LeadingAndTrailing()
    {
        const string path = "~One~";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"~One~\"", actual);
    }

    [Fact]
    public void Tilde_Trailing()
    {
        const string path = "Two~";

        var actual = _transformation.Transform(path);

        Assert.Equal("\"Two~\"", actual);
    }
}
