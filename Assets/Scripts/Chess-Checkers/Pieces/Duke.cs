public class Duke : Checker
{
    public override void InstantiatePieceComponents(bool forP1)
    {
        this._MeshRenderer.material = forP1 ? Board_SO.Piece_p1Color : Board_SO.Piece_p2Color;
        this.TeamID = forP1 ? Board_SO.P1_DUKE : Board_SO.P2_DUKE;
    }
}