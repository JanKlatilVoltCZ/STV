namespace Resolver
{
    public class Candidate
    {
        public string Name { get; }
        public int Id { get; }
        public Candidate(string name, int id)
        {
            Name = name;
            Id = id;
        }
    }
}
