namespace PowerPointClone.InterfaceQualite.RevisionCommentaires;
public sealed class CommentaireDiapositive
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int NumeroDiapositive { get; set; }
    public string Auteur { get; set; } = Environment.UserName;
    public string Texte { get; set; } = "";
    public DateTime CreeLe { get; init; } = DateTime.Now;
    public bool Resolu { get; private set; }
    public void MarquerResolu() => Resolu = true;
    public void Rouvrir() => Resolu = false;
}