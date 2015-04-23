using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parameters
{
    public static class tagfield_parameters
    {
        public static int minimalTagLength = 3;
        public static int maximalTagLength = 127;
        public static int maximumTagsNumber = 1024;
    }
}

namespace TDA
{

    public class TagField
    {

        public struct TagNode
        {
            public string name;
            public UInt64 ID;
        }

        private UInt64 nextTagID;

        private LinkedList<TagNode> tags;

        public TagField()
        {
            tags = new LinkedList<TagNode>();
        }

        public int readFromFile(System.IO.Stream str)
        {
            System.IO.StreamReader strr = new System.IO.StreamReader(str);
            string line = strr.ReadLine();
            if (line == null) return (1);
            nextTagID = UInt64.Parse(line);
            TagNode inserter;
            string[] splitter;
            while ((line = strr.ReadLine()) != null)
            {
                splitter = line.Split(' ');
                inserter.name = splitter[0];
                inserter.ID = UInt64.Parse(splitter[1]);
                tags.AddLast(inserter);
            }
            str.Close();
            return (0);
        }

        public int writeToFile(System.IO.Stream str)
        {
            System.IO.StreamWriter strw = new System.IO.StreamWriter(str);
            strw.WriteLine(nextTagID.ToString());
            foreach(TagNode tag in tags)
            {
                strw.WriteLine(tag.name + " " + tag.ID.ToString());
            }
            strw.Flush();
            str.Close();
            return (0);
        }

        public LinkedList<TagNode> returnSearch(string request)
        {
            if (request.Length == 0) return (tags);
            LinkedList<TagNode> target = new LinkedList<TagNode>();
            foreach(TagNode node in tags)
            {
                if (node.name.Contains(request)) target.AddLast(node);
            }
            return (target);
        }

        public static bool charCheck(string input)
        {
            if ((input.Length < parameters.tagfield_parameters.minimalTagLength)
                || (input.Length > parameters.tagfield_parameters.maximalTagLength)) return (false);
            bool matches = true;
            int charcode = 0;
            foreach(char symbol in input)
            {
                charcode = (int)symbol;
                if (!(((charcode <= (int)'z') && (charcode >= (int)'a')) || ((charcode <= (int)'Z') && (charcode >= (int)'A'))
                    || (charcode == (int)'_') || ((charcode <= (int)'9') && (charcode >= (int)'1')) || (charcode == (int)'0')))
                {
                    matches = false;
                    break;
                }
            }
            if (matches) return (true);
            else return (false);
        }

        public int addTag(string input)
        {
            if (tags.Count == parameters.tagfield_parameters.maximumTagsNumber) return (3);
            if(!charCheck(input)) return (1);
            int index = 0;
            int comparer = -1;
            bool inserted = false;
            while(true)
            {
                if(index >= tags.Count)
                {
                    inserted = false;
                    break;
                }
                comparer = String.Compare(input, tags.ElementAt(index).name, true, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                if (comparer == 0) return (2);
                if (comparer > 0)
                {
                    TagNode inserter = new TagNode();
                    inserter.name = input;
                    inserter.ID = nextTagID;
                    nextTagID++;
                    tags.AddAfter(tags.Find(tags.ElementAt(index)), inserter);
                    inserted = true;
                    break;
                }
                index++;
            }
            if (inserted == false)
            {
                TagNode inserter;
                inserter.name = input;
                inserter.ID = nextTagID;
                nextTagID++;
                tags.AddLast(inserter);
            }
            return (0);
        }

        public int removeTag(string input)
        {
            if (tags.Count != 0)
            {
                LinkedListNode<TagNode> index = tags.First;
                while(true)
                {
                    if (index.Value.name == input)
                    {
                        tags.Remove(index);
                        return (0);
                    }
                    if (index == tags.Last) break;
                    index = index.Next;
                }
            }
            return (1);
        }
    }
}
