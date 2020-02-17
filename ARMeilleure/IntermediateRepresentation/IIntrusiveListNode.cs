namespace ARMeilleure.IntermediateRepresentation
{
    interface IIntrusiveListNode<T> where T : class
    {
        T ListPrevious { get; set; }
        T ListNext { get; set; }
    }
}
