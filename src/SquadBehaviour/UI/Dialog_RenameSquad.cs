using Verse;

namespace SquadBehaviour
{
    public class Dialog_RenameSquad : Dialog_Rename<Squad>
    {
        public Dialog_RenameSquad(Squad area) : base(area)
        {

        }

        protected override AcceptanceReport NameIsValid(string name)
        {
            AcceptanceReport result = base.NameIsValid(name);
            if (!result.Accepted)
            {
                return result;
            }
            return true;
        }
    }
}