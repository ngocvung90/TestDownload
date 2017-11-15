namespace AngleSharp.Performance.Html
{
    using AngleSharp;
    using AngleSharp.Parser.Html;
    using System;

    class AngleSharpParser : ITestee
    {
        static readonly IConfiguration configuration = new Configuration();
        static readonly HtmlParser parser = new HtmlParser(configuration);

        public String Name
        {
            get { return "AngleSharp"; }
        }

        public Type Library
        {
            get { return typeof(HtmlParser); }
        }

        public HtmlParser CurrentParser
        {
            get
            {
                return parser;
            }
        }
        public void Run(String source, bool isHTML = true)
        {
            parser.Parse(source, isHTML);
        }
    }
}
