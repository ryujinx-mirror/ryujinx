namespace ARMeilleure.IntermediateRepresentation
{
    interface IIntrusiveListNode<T>
    {
        T ListPrevious { get; set; }
        T ListNext { get; set; }
    }
}
