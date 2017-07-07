# Q-Zone

Introduction
============
This project contains a sample web application for online MCQ exam system. As a sample it only contains the backend part of the web application.


How to use
==========
## Prerequisites
The project code might be used in various environments after some sort of tweaking but it has been tested in a specific environment. The following tools has been used in the test environment:
  - **Programming IDE:** *Visual Studio 2015*
  - **Programming Framework:** *ASP.NET 4.5* (*MVC* is preferable but *Web Forms* should work just fine)
  - **Database Tool:** 
    - *Oracle Database 12c* with *Oracle Developer Tools for Visual Studio*
    - *Navicat* (optional, just needed to redesign the ERD of the database)

## Database Configuration
  - **Configure:** Please refer to `Database/_Readme.txt` file to install or uninstall database configuration.
  - **Edit:** You can directly edit the database scripts in `Database/Scripts` folder. But for convenience, please open `Database/Q-Zone.ndm` file with *Navicat* and edit the database within the app.

## Run Project
After performing the above, you can run easily the project. Please do the followings to run the project:
  - Open `Q-Zone.sln` with *Visual Studio 2015*.
  - Add a new *ASP.NET* project to the solution. You can use either *MVC* or *Web Forms* but *MVC* is preferable.
  - Add some code to integrate with `Q-Zone.Models`. Please refer to [Code Overview](#code-overview) section for more details.


Code Overview
=============
To view the summary of `Q-Zone.Models`, please refer to `Q-Zone.Models/Models.txt` file. Here, the in-depth code overview has been discussed.

## Data Connector (`Q-Zone.Models/DataConnector/DataAccessLayer.cs`)
It connects the other parts of this project with Oracle database. Cool thing is: the data connector alone can be used in other projects for the same purpose. In this case, you just need to remove the references of `Q_Zone` and all should be fine.

## User Authorization (`Q-Zone.Models/Account/UserAuthorization.cs`)
To sign up, login, logout or delete own account, you need the code. Most of the methods of `UserAuthorization` have their own documentations, though method names are self-explanatory.

## User (`Q-Zone.Models/Account/User.cs`)
User can view or change his/her own email, password or name. Most of the methods have their own documentations, though method names are self-explanatory.

## Topic Bank (`Q-Zone.Models/Question/TopicBank.cs`)
User can add or view his/her own topics. He/She can also search for some specific topics in his/her topic bank. To delete a topic from the user, please use `Delete()` method of the corresponding `Topic` instance.

## Topic (`Q-Zone.Models/Question/Topic.cs`)
User can view all the questions in a topic. He/She can change the topic name or even delete the topic along with all the containing questions and quizzes. He/She can also search some specific questions in the topic. To add a new question, call `QuestionBank.AddQuestion(topic)` passing the corresponding `Topic` instance.

## QuestionBank (`Q-Zone.Models/Question/QuestionBank.cs`)
It is very similar to `Topic Bank`. User can add, view or search questions. Like `Topic Bank`, to delete a question, use `Delete()` method of the corresponding `Question` instance.

## Question (`Q-Zone.Models/Question/Question.cs`)
User can perform various operations to his/her own question. Most of the methods have their own documentations, though method names are self-explanatory.

## Quiz Bank (`Q-Zone.Models/Quiz/QuizBank.cs`)
It is very similar to `Topic Bank` or `Question Bank`.

## Quiz (`Q-Zone.Models/Quiz/Quiz.cs`)
User can perform various operations to his/her own quiz. Most of the methods have their own documentations, though method names are self-explanatory. Student user has to use `Answer Sheet` to give answers to the questions of a quiz.

## Answer Sheet (`Q-Zone.Models/Quiz/AnswerSheet.cs`)
During a quiz, student user has to use this class. Student user must have to call `Submit()` before timeout of the quiz. Most of the methods have their own documentations, though method names are self-explanatory.

## Result (`Q-Zone.Models/Quiz/Result.cs`)
The name is self-explanatory. Besides most of the methods have their own documentations, though method names are self-explanatory.

## Rank List (`Q-Zone.Models/Quiz/RankList.cs`)
It contains some useful methods related to rank list. All of the methods have their own documentations, though method names are self-explanatory.

## Utility (`Q-Zone.Models/Utility/Encryption.cs`)
Here are 2 files:
  - `Encryption.cs` has been used in store encrypted passwords in the database. But using it, you may encrypt other things.
  - `ValidFormat.cs` has been used to check valid email format for database entry.

## Unit Testing (`Q-Zone.Tests`)
This is for performing unit test covering many methods of `Q-Zone.Models`. As unit testing framework, it uses *MSTest* which comes with *Visual Studio 2015*.


Features
========
## Login / Sign up / Logout
  - Username, password, email
  - Change password using current password
  - Change email or name

## Creating Question Bank
  - Add questions (all have fixed marks and minus marks)
    - Select topic
    - Write question
    - Add/Delete options (total 5 options)
      - Add correct answer first
      - Add 4 incorrect options
      - Edit options
    - Select difficulty level
  - View self-created questions
    - By topic
      - By browsing questions
      - By searching phrase or difficulty
  - Edit and delete questions (options shown next to questions)

## Creating and Assigning Quizzes
  - Select topic
  - Add random questions (ordering of options may be different for different participants)
    - Choose the number of random questions
    - Choose by difficulty or undefined difficulty
  - Add fixed questions (ordering of options may be different for different participants)
    - By browsing questions
    - By searching phrase or difficulty
  - View added questions
  - Delete questions
  - Reorder questions
  - Set date, time and duration
  - Receive quiz ID
  - View self-created quizzes
    - By browsing quizzes
      - By date or topic
    - By quiz ID
      - If not self-created, then inform user and show nothing
  - Edit quizzes that have not happened yet (option shown next to quizzes)
  - Delete quizzes

## Participating in Quizzes
  - Enter quiz ID
  - Show countdown timer
    - Before quiz
    - During quiz
  - After submitting all quiz answers
    - Receive marks and percent
    - Receive correct answers for quiz questions
  - Show link to rank list if quiz duration is over

## Viewing Rank Lists
  - View rank lists for past quizzes (does not matter if user is a participant or not)
    - By browsing participated quizzes
      - By date or topic
    - By quiz ID
      - If quiz is not over, then inform user and show nothing
  - Highlight own rank among others if participated
  - View correct answers for quiz questions

