using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatApp_Part3
{
    public class QuizQuestion
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectAnswer { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class QuizResult
    {
        public bool IsCorrect { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public bool QuizComplete { get; set; }
    }

    public class QuizMaster
    {
        private List<QuizQuestion> questions;
        private Queue<QuizQuestion> questionQueue;
        private int currentQuestionIndex = 0;
        private int score = 0;
        private int totalQuestions = 0;
        private LogBook? logger;

        public int Score => score;
        public int TotalQuestions => totalQuestions;

        public QuizMaster(LogBook? logger = null)
        {
            this.logger = logger;
            questions = LoadQuestions();
            questionQueue = new Queue<QuizQuestion>(questions);
            totalQuestions = questions.Count;
        }

        private List<QuizQuestion> LoadQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    CorrectAnswer = 2,
                    Explanation = "Reporting phishing emails helps prevent scams and protects others.",
                    Category = "Phishing"
                },
                new QuizQuestion
                {
                    Question = "What is the strongest type of password?",
                    Options = new List<string> { "A short password with special characters", "A long passphrase with a mix of characters", "Your birthdate", "A common word with numbers" },
                    CorrectAnswer = 1,
                    Explanation = "Long passphrases create much higher entropy and are harder to crack.",
                    Category = "Passwords"
                },
                new QuizQuestion
                {
                    Question = "Which of the following is a sign of a phishing attempt?",
                    Options = new List<string> { "Professional design", "Urgent requests for personal information", "Familiar sender address", "Attachments from known contacts" },
                    CorrectAnswer = 1,
                    Explanation = "Urgency is a common phishing tactic to pressure you into acting without thinking.",
                    Category = "Phishing"
                },
                new QuizQuestion
                {
                    Question = "What does MFA (Multi-Factor Authentication) require?",
                    Options = new List<string> { "Only a password", "Two or more verification methods", "A single-use code", "A hardware token only" },
                    CorrectAnswer = 1,
                    Explanation = "MFA requires at least two factors: something you know, have, or are.",
                    Category = "Passwords"
                },
                new QuizQuestion
                {
                    Question = "What should you do if you find an unknown USB drive in the office?",
                    Options = new List<string> { "Plug it in to check its contents", "Give it to security without plugging it in", "Test it on your personal device", "Ignore it" },
                    CorrectAnswer = 1,
                    Explanation = "Unknown USB drives can contain malware. Always hand them to security.",
                    Category = "Malware"
                },
                new QuizQuestion
                {
                    Question = "Which of these is a secure way to browse online?",
                    Options = new List<string> { "Using HTTP sites only", "Checking for HTTPS and the padlock", "Clicking all links in emails", "Using the same password everywhere" },
                    CorrectAnswer = 1,
                    Explanation = "HTTPS encrypts your data. Always check for the padlock symbol.",
                    Category = "Browsing"
                },
                new QuizQuestion
                {
                    Question = "What is social engineering?",
                    Options = new List<string> { "A programming technique", "Manipulating people to reveal information", "A type of virus", "A security protocol" },
                    CorrectAnswer = 1,
                    Explanation = "Social engineering exploits human behavior rather than technical vulnerabilities.",
                    Category = "Social Engineering"
                },
                new QuizQuestion
                {
                    Question = "What should you do if you suspect your account has been compromised?",
                    Options = new List<string> { "Wait and see what happens", "Change your password immediately", "Ignore it if nothing seems wrong", "Message the attacker" },
                    CorrectAnswer = 1,
                    Explanation = "Immediately change your password and enable MFA to secure your account.",
                    Category = "Incident Response"
                },
                new QuizQuestion
                {
                    Question = "What's the best practice for storing passwords?",
                    Options = new List<string> { "Write them down", "Use a password manager", "Save them in your browser", "Use the same password everywhere" },
                    CorrectAnswer = 1,
                    Explanation = "Password managers securely store and generate unique passwords for each site.",
                    Category = "Passwords"
                },
                new QuizQuestion
                {
                    Question = "What does 'phishing' refer to?",
                    Options = new List<string> { "A type of computer virus", "A fraudulent attempt to steal personal information", "A programming language", "A security protocol" },
                    CorrectAnswer = 1,
                    Explanation = "Phishing is a social engineering attack that tricks people into revealing sensitive information.",
                    Category = "Phishing"
                },
                new QuizQuestion
                {
                    Question = "Which of these is a strong password?",
                    Options = new List<string> { "password123", "Blue!Ocean99#Bridge", "john1980", "secure" },
                    CorrectAnswer = 1,
                    Explanation = "A long passphrase with mixed characters is much stronger than common passwords.",
                    Category = "Passwords"
                },
                new QuizQuestion
                {
                    Question = "What is ransomware?",
                    Options = new List<string> { "A type of antivirus software", "Malware that encrypts files and demands payment", "A security update", "A hardware device" },
                    CorrectAnswer = 1,
                    Explanation = "Ransomware locks your files and demands a ransom for their release.",
                    Category = "Malware"
                },
                new QuizQuestion
                {
                    Question = "Why should you check the URL before clicking a link?",
                    Options = new List<string> { "It loads faster", "To ensure it's a legitimate site", "It's required by law", "To check the design" },
                    CorrectAnswer = 1,
                    Explanation = "Checking URLs helps identify fake sites designed to steal your information.",
                    Category = "Browsing"
                },
                new QuizQuestion
                {
                    Question = "What is the 3-2-1 backup rule?",
                    Options = new List<string> { "3 backups daily, 2 weekly, 1 monthly", "3 copies, 2 storage types, 1 stored offsite", "3 TB minimum, 2 drives, 1 cloud", "Backup 3x a week at 2 AM" },
                    CorrectAnswer = 1,
                    Explanation = "Keep 3 copies on 2 different media, with 1 copy stored offsite for disaster recovery.",
                    Category = "Incident Response"
                },
                new QuizQuestion
                {
                    Question = "What should you do if you receive a suspicious text message?",
                    Options = new List<string> { "Reply for more information", "Click any links to verify", "Delete it and report it", "Forward it to friends" },
                    CorrectAnswer = 2,
                    Explanation = "Don't interact with suspicious messages. Delete and report them.",
                    Category = "Social Engineering"
                }
            };
        }

        public QuizQuestion? StartQuiz()
        {
            if (questionQueue.Count == 0)
            {
                var shuffled = questions.OrderBy(x => Guid.NewGuid()).Take(10).ToList();
                questionQueue = new Queue<QuizQuestion>(shuffled);
                score = 0;
                currentQuestionIndex = 0;
                totalQuestions = shuffled.Count;
            }

            if (questionQueue.Count == 0) return null;

            var question = questionQueue.Peek();
            logger?.Log($"Quiz started - Question {currentQuestionIndex + 1}/{totalQuestions}",
                $"Category: {question.Category}");
            return question;
        }

        public QuizResult AnswerQuestion(int selectedIndex)
        {
            if (questionQueue.Count == 0)
            {
                return new QuizResult
                {
                    IsCorrect = false,
                    Explanation = "No active question.",
                    QuizComplete = true
                };
            }

            var question = questionQueue.Dequeue();
            bool isCorrect = selectedIndex == question.CorrectAnswer;

            if (isCorrect)
                score++;

            currentQuestionIndex++;

            var result = new QuizResult
            {
                IsCorrect = isCorrect,
                Explanation = question.Explanation,
                QuestionText = question.Question,
                QuizComplete = questionQueue.Count == 0
            };

            logger?.Log($"Quiz answer: {(isCorrect ? "Correct" : "Incorrect")}",
                $"Question: {question.Question}");

            if (result.QuizComplete)
            {
                logger?.Log($"Quiz Complete", $"Final Score: {score}/{totalQuestions}");
            }

            return result;
        }

        public string GetPerformanceMessage()
        {
            double percentage = (double)score / totalQuestions * 100;
            return percentage >= 90 ? "🏆 Outstanding! You're a cybersecurity expert!" :
                   percentage >= 75 ? "🌟 Great job! You have strong security knowledge!" :
                   percentage >= 60 ? "👍 Good work! Keep learning to improve!" :
                   percentage >= 40 ? "📚 Review the concepts and try again." :
                   "🔁 Needs work. Review the security fundamentals and retry.";
        }
    }
}