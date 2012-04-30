using System.Collections.Generic;
using System.Collections.Specialized;

namespace Hub.Dsl
{
	public sealed class Context
	{
		public IDictionary<string,Fragment> Fragments = new Dictionary<string, Fragment>();
		public IList<Message> Contracts = new List<Message>();
        //public IDictionary<string,string> Modifiers = new Dictionary<string, string>();

        public Stack<Entity> Entities { get; set; }

        public Entity CurrentEntity { get { return Entities.Peek(); } }

	    public Context()
	    {
            Entities = new Stack<Entity>();
	        var entity = new Entity("default");
            entity.Modifiers.Add("?", "ICommand");
            entity.Modifiers.Add("!", "IEvent");



	        Entities.Push(entity);
	    }
	}

    public sealed class Entity
    {
        public string Name { get; set; }
        public List<Member> FixedMembers { get; set; }
        public List<Message> Messages { get; set; }
        public NameValueCollection Modifiers { get; set; }

        public Entity(string name)
        {
            Name = name;
            FixedMembers = new List<Member>();
            Messages = new List<Message>();
            Modifiers = new NameValueCollection();
        }
    }

    public sealed class Fragment
    {
        public readonly string Type;
        public readonly string Name;

        public Fragment(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }


    public sealed class Member
    {
        public readonly string Name;
        public readonly string Type;

        public Member(string type, string name)
        {
            Name = name;
            Type = type;
        }
    }

    public sealed class Message
    {
        public readonly string Name;
        public readonly IList<Modifier> Modifiers;

        public Message(string name, IList<Modifier> modifiers)
        {
            Name = name;
            Modifiers = modifiers;
        }

        public List<Member> Members = new List<Member>();
    }

    public sealed class Modifier
    {
        public readonly string Identifier;
        public readonly string Interface;

        public Modifier(string identifier, string @interface)
        {
            Identifier = identifier;
            Interface = @interface;
        }
    }


}