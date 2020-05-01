namespace DomainModel
{
    /// <summary>
    /// Instruction query object
    /// </summary>
    public class InstructionQuery : Query
    {
        /// <summary>
        /// The query's instruction
        /// </summary>
        public string Instruction { get; set; }
        
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public InstructionQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_INSTRUCTION;
        }

        /// <summary>
        /// Create a new Instruction query with the provided instruction
        /// </summary>
        /// <param name="instruction"></param>
      //  public InstructionQuery(string instruction)
       // {
       //     Type = Constants.TYPE_INSTRUCTION;
       //     Instruction = instruction;
       // }
    }
}