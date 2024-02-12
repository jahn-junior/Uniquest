using System.Collections.Generic;
using System.Linq;
using Random = System.Random;
using UnityEngine;

namespace Singleton
{
    public class QuestionFactory : MonoBehaviour
    {
        private static QuestionFactory INSTANCE;
        
        private static readonly Maze MAZE;
        private static readonly Random RANDOM = new();
        private static DataService DATA_SERVICE;
        
        private QuestionWindowController myQuestionWindowController;
        private Question myCurrentQuestion;
        private IEnumerable<Question> myQuestions;
        private List<Question> myRandomizedQuestions;
        private bool isNewGame = true;
        
        void Start()
        {
            if (isNewGame)
            {
                DATA_SERVICE = new DataService("data.sqlite");
                InitializeQuestionArray();
                myQuestionWindowController = GetComponent<QuestionWindowController>();
                isNewGame = false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private void Awake()
        {
            if (INSTANCE != null && INSTANCE != this)
            {
                Debug.Log("There is already an instance of the factory in the scene");
            }
            else
            {
                MyInstance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        
        private void InitializeQuestionArray()
        {
            // myQuestions = myDataService.GetQuestion();
            myQuestions = DATA_SERVICE.GetQuestions().Where(q => !q.myIsAnswered);
            // myQuestions = DATA_SERVICE.GetQuestion();
            myRandomizedQuestions = myQuestions.OrderBy(a => RANDOM.Next()).ToList();
        }


        public void DisplayWindow()
        {
            myCurrentQuestion = GetRandomQuestion();
            myQuestionWindowController.InitializeWindow(myCurrentQuestion);
        }

        private Question GetRandomQuestion()
        {
            if (myQuestions != null)
            {
                myCurrentQuestion = myRandomizedQuestions[0];
                return myCurrentQuestion;
            }
            return null;
        }

        public void RemoveCurrentQuestion()
        {
            if (myRandomizedQuestions != null)
            {
                myRandomizedQuestions.RemoveAll(x=>x.MyQuestion == myCurrentQuestion.MyQuestion);
            }
            else
            {
                Debug.Log("The question list is empty!");
            }
        }
        
        public static QuestionFactory MyInstance
        {
            get => INSTANCE;
            private set => INSTANCE = value;
        }

        public IEnumerable<Question> GetQuestions()
        {
            return myRandomizedQuestions;
        }
        
        public void MarkQuestionAsAnswered()
        {
            myCurrentQuestion.myIsAnswered = true;
            DATA_SERVICE.MarkQuestionAsAnswered(myCurrentQuestion.MyQuestionID);
        }
        
        public void InitializeQuestionsFromSave()
        {
            var allQuestions = DATA_SERVICE.GetQuestion();
            myQuestions = allQuestions.Where(q => PlayerPrefs.GetInt("QuestionAnswered_" + q.MyQuestionID, 0) == 0);
            myRandomizedQuestions = myQuestions.OrderBy(a => RANDOM.Next()).ToList();
        }
    }
}