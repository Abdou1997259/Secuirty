namespace Secuirty.Dtos
{
    public class FaceResultDto
    {
        public FaceBookData Data { get; set; }
    }
    public class FaceBookData
    {
        public bool Is_Valid { get; set; }
        public string User_Id { get; set; }
    }
}
