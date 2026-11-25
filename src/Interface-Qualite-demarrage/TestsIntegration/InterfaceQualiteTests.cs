using Xunit;
using PowerPointClone.InterfaceQualite.AffichageReglesGrilleZoom;
using PowerPointClone.InterfaceQualite.ConceptionTailleArrierePlan;
using PowerPointClone.InterfaceQualite.RevisionCommentaires;
using PowerPointClone.InterfaceQualite.RevisionOrthographe;
namespace PowerPointClone.InterfaceQualite.Tests;
public sealed class InterfaceQualiteTests
{
    [Fact] public void Zoom_Est_Limite_Entre_10_Et_400()
    { var vm = new AffichageReglesGrilleZoomViewModel { Zoom = 900 }; Assert.Equal(400, vm.Zoom); }
    [Fact] public void Zoom_Reinitialise_A_100()
    { var vm = new AffichageReglesGrilleZoomViewModel(); vm.Zoom = 150; vm.ZoomReinitialiser(); Assert.Equal(100, vm.Zoom); }
    [Fact] public void Format_Refuse_Les_Dimensions_Negatives()
    { var f = new FormatDiapositive(); Assert.Throws<ArgumentOutOfRangeException>(() => f.DefinirFormat(-1, 5)); }
    [Fact] public void Commentaire_Peut_Etre_Resolu()
    { var c = new CommentaireDiapositive(); c.MarquerResolu(); Assert.True(c.Resolu); }
    [Fact] public void Orthographe_Retourne_Les_Mots_Inconnus()
    { var s = new OrthographeService(); Assert.Contains("zztest", s.TrouverMotsInconnus("bonjour zztest")); }
}