# ChatApp_PART_3

🛡 SecureShield Awareness System

SecureShield is a WPF desktop application that teaches cybersecurity awareness through a conversational chatbot, interactive quizzes, timed security drills, and a personal security task tracker. It started life as a console-based bot and has grown into a full chat-style assistant with a dark, terminal-inspired UI.

Features


Conversational security assistant — Chat naturally with "Shield," a rule-based assistant that recognizes topics like phishing, passwords/MFA, secure browsing, malware, privacy, and social engineering, and responds with tailored briefings.
Mood-aware responses — The assistant detects tone in your messages (calm, alarmed, irritated, or inquisitive) and adjusts how it responds accordingly.
Security drills — Take a randomized 5-question drill with instant feedback, explanations, and a final score/rating.
Knowledge quiz — A separate 10-question multiple-choice quiz pulled from a 15-question bank covering core security topics, with running score and performance feedback.
Security task tracker — Add, complete, delete, and set reminders for personal security to-dos (e.g. "enable two-factor authentication"), backed by SQL Server with automatic in-memory fallback if the database is unavailable.
Activity log — A running, timestamped log of everything that happens in the app (modules viewed, tasks added, quiz results, drills completed), viewable and clearable from its own panel.
Agent profile — View a summary of which topics you've explored, how often, and your overall drill performance.
Persistent chat history — All conversations are written to a local log file (secureshield_log.txt) and can be recalled in-app by typing log.


Tech Stack


.NET 10 (net10.0-windows)
WPF (Windows Presentation Foundation) for the UI
System.Data.SqlClient for SQL Server task persistence
C# with nullable reference types and implicit usings enabled


Project Structure

FileResponsibilityApp.xaml / App.xaml.csApplication entry point and resourcesMainWindow.xaml / MainWindow.xaml.csMain UI: chat panel, tabs (Chat / Tasks / Quiz / Activity Log), message rendering, and event wiringSecurityBot.csConsole-style startup sequence: banner, startup audio, agent name capture, greetingThreatResponseEngine.csCore chatbot logic: keyword-based topic dispatch, mood detection, drills, help menu, profile reports, and file loggingTaskStore.csCRUD operations for security tasks, backed by SQL Server with in-memory fallbackLogBook.csIn-memory, capped activity log with formatted output and change notificationsQuizMaster.csQuiz question bank, session management, scoring, and performance messaging

Getting Started

Prerequisites


Windows OS
.NET 10 SDK with the WPF workload
(Optional) SQL Server instance for persistent task storage — the app falls back to in-memory tasks if no database is reachable


Configure the database (optional)

By default, TaskStore connects using:

Server=localhost;Database=SecureShieldDB;Integrated Security=True;

To use a different connection string, add a SecureShieldDB entry to your app configuration's connectionStrings section. The Tasks table should include the columns: Id, Title, Description, CreatedAt, ReminderDate, IsComplete.

Build and run

bashdotnet build
dotnet run

Or open ChatApp_Part3.slnx in Visual Studio and run the project directly.

Usage

On launch, the assistant introduces itself and asks for your agent name. From there, you can:


Type a topic (phishing, password, browsing, privacy, malware, social, incident) for a briefing
Type drill to start a 5-question security drill
Type help or menu for the full command list
Type profile to see your stats
Type log to view recent chat history
Type add task: <description> to create a new security task, then remind <days> to set a reminder on it
Type show tasks or complete task for quick task actions
Switch to the Quiz tab to take the standalone knowledge quiz
Switch to the Activity Log tab to review and clear logged actions


Notes


An optional welcome.wav file placed in the application's base directory will be played on startup.
Chat history is appended to secureshield_log.txt in the application directory, with session open/close markers.
Activity logs are capped at 100 entries; chat history is capped at 200 entries to keep memory usage bounded.
