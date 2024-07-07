
namespace Room4 {
    public class ShuffledQuestionHandler<T> : BaseQuestionHandler<T> {
        protected override void Start() {
            base.Start();
            Shuffle(questionList);
        }
    }
}
