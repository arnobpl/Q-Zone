----------------------------------------
-- Delete previous tables
----------------------------------------
ALTER TABLE Question DROP CONSTRAINT Question_Correct_Answer_ID CASCADE;
ALTER TABLE Answer_Option DROP CONSTRAINT Answer_Option_Question_ID CASCADE;
ALTER TABLE Question DROP CONSTRAINT Question_Topic_ID CASCADE;
ALTER TABLE Topic DROP CONSTRAINT Topic_Owner_ID CASCADE;
ALTER TABLE Quiz DROP CONSTRAINT Quiz_Topic_ID CASCADE;
ALTER TABLE Quiz_Question DROP CONSTRAINT Quiz_Question_Quiz_ID CASCADE;
ALTER TABLE Quiz_Question DROP CONSTRAINT Quiz_Question_Question_ID CASCADE;
ALTER TABLE Quiz_Result DROP CONSTRAINT Result_Owner_ID CASCADE;
ALTER TABLE Quiz_Result DROP CONSTRAINT Result_Quiz_ID CASCADE;
ALTER TABLE Answer_Sheet DROP CONSTRAINT Answer_Sheet_Result_ID CASCADE;
ALTER TABLE Answer_Sheet DROP CONSTRAINT Answer_Sheet_Question_ID CASCADE;
ALTER TABLE Answer_Sheet DROP CONSTRAINT Answer_Sheet_Option_ID CASCADE;

DROP INDEX Question_String_Lowercase;
DROP INDEX Topic_Name_Lowercase;
DROP INDEX Quiz_Name_Lowercase;

ALTER TABLE Login DROP CONSTRAINT User_Username CASCADE;
ALTER TABLE Login DROP CONSTRAINT User_Email CASCADE;
ALTER TABLE Answer_Option DROP CONSTRAINT Question_Option_String CASCADE;
ALTER TABLE Topic DROP CONSTRAINT Topic_Name_Owner_ID CASCADE;
ALTER TABLE Quiz_Question DROP CONSTRAINT Quiz_Question_ID CASCADE;
ALTER TABLE Answer_Sheet DROP CONSTRAINT Answer_Question_Option CASCADE;
ALTER TABLE Quiz_Result DROP CONSTRAINT Result_Owner_Quiz_ID CASCADE;

DROP TABLE Login CASCADE CONSTRAINTS;
DROP TABLE Question CASCADE CONSTRAINTS;
DROP TABLE Answer_Option CASCADE CONSTRAINTS;
DROP TABLE Topic CASCADE CONSTRAINTS;
DROP TABLE Quiz CASCADE CONSTRAINTS;
DROP TABLE Quiz_Question CASCADE CONSTRAINTS;
DROP TABLE Answer_Sheet CASCADE CONSTRAINTS;
DROP TABLE Quiz_Result CASCADE CONSTRAINTS;

----------------------------------------
-- Create new tables
----------------------------------------
CREATE TABLE Login (
User_ID NUMBER NOT NULL,
Username VARCHAR2(255) NOT NULL,
Password VARCHAR2(255) NOT NULL,
Email VARCHAR2(255) NOT NULL,
Name VARCHAR2(255) NULL,
PRIMARY KEY (User_ID) 
);

ALTER TABLE Login ADD CONSTRAINT User_Username UNIQUE (Username);
ALTER TABLE Login ADD CONSTRAINT User_Email UNIQUE (Email);

CREATE TABLE Question (
Question_ID NUMBER NOT NULL,
Topic_ID NUMBER NOT NULL,
Question_String VARCHAR2(255) NULL,
Correct_Answer_ID NUMBER NULL,
Difficulty_Level NUMBER NULL,
PRIMARY KEY (Question_ID) 
);

CREATE INDEX Question_String_Lowercase ON Question (LOWER(Question_String));

CREATE TABLE Answer_Option (
Option_ID NUMBER NOT NULL,
Question_ID NUMBER NOT NULL,
Option_String VARCHAR2(255) NULL,
PRIMARY KEY (Option_ID) 
);

ALTER TABLE Answer_Option ADD CONSTRAINT Question_Option_String UNIQUE (Question_ID, Option_String);

CREATE TABLE Topic (
Topic_ID NUMBER NOT NULL,
Owner_ID NUMBER NOT NULL,
Topic_Name VARCHAR2(255) NOT NULL,
Total_Questions NUMBER DEFAULT 0 NOT NULL,
PRIMARY KEY (Topic_ID) 
);

ALTER TABLE Topic ADD CONSTRAINT Topic_Name_Owner_ID UNIQUE (Owner_ID, Topic_Name);
CREATE INDEX Topic_Name_Lowercase ON Topic (LOWER(Topic_Name));

CREATE TABLE Quiz (
Quiz_ID NUMBER NOT NULL,
Topic_ID NUMBER NOT NULL,
Quiz_Name VARCHAR2(255) NULL,
Date_Time DATE NULL,
Duration_Day NUMBER NULL,
Is_Public NUMBER(1) DEFAULT 0 NOT NULL,
Total_Participants NUMBER DEFAULT 0 NOT NULL,
Average_Marks NUMBER DEFAULT 0 NOT NULL,
PRIMARY KEY (Quiz_ID) 
);

CREATE INDEX Quiz_Name_Lowercase ON Quiz (LOWER(Quiz_Name));

CREATE TABLE Quiz_Question (
Quiz_Question_ID NUMBER NOT NULL,
Quiz_ID NUMBER NOT NULL,
Question_ID NUMBER NOT NULL,
Question_Order NUMBER NULL,
PRIMARY KEY (Quiz_Question_ID) 
);

ALTER TABLE Quiz_Question ADD CONSTRAINT Quiz_Question_ID UNIQUE (Quiz_ID, Question_ID);

CREATE TABLE Answer_Sheet (
Question_Answer_ID NUMBER NOT NULL,
Result_ID NUMBER NOT NULL,
Question_ID NUMBER NOT NULL,
Option_ID NUMBER NOT NULL,
PRIMARY KEY (Question_Answer_ID) 
);

ALTER TABLE Answer_Sheet ADD CONSTRAINT Answer_Question_Option UNIQUE (Result_ID, Question_ID);

CREATE TABLE Quiz_Result (
Result_ID NUMBER NOT NULL,
Owner_ID NUMBER NOT NULL,
Quiz_ID NUMBER NOT NULL,
Obtained_Marks NUMBER NULL,
Percentage NUMBER NULL,
PRIMARY KEY (Result_ID) 
);

ALTER TABLE Quiz_Result ADD CONSTRAINT Result_Owner_Quiz_ID UNIQUE (Owner_ID, Quiz_ID);


ALTER TABLE Question ADD CONSTRAINT Question_Correct_Answer_ID FOREIGN KEY (Correct_Answer_ID) REFERENCES Answer_Option (Option_ID) ON DELETE CASCADE;
ALTER TABLE Answer_Option ADD CONSTRAINT Answer_Option_Question_ID FOREIGN KEY (Question_ID) REFERENCES Question (Question_ID) ON DELETE CASCADE;
ALTER TABLE Question ADD CONSTRAINT Question_Topic_ID FOREIGN KEY (Topic_ID) REFERENCES Topic (Topic_ID) ON DELETE CASCADE;
ALTER TABLE Topic ADD CONSTRAINT Topic_Owner_ID FOREIGN KEY (Owner_ID) REFERENCES Login (User_ID) ON DELETE CASCADE;
ALTER TABLE Quiz ADD CONSTRAINT Quiz_Topic_ID FOREIGN KEY (Topic_ID) REFERENCES Topic (Topic_ID) ON DELETE CASCADE;
ALTER TABLE Quiz_Question ADD CONSTRAINT Quiz_Question_Quiz_ID FOREIGN KEY (Quiz_ID) REFERENCES Quiz (Quiz_ID) ON DELETE CASCADE;
ALTER TABLE Quiz_Question ADD CONSTRAINT Quiz_Question_Question_ID FOREIGN KEY (Question_ID) REFERENCES Question (Question_ID) ON DELETE CASCADE;
ALTER TABLE Quiz_Result ADD CONSTRAINT Result_Owner_ID FOREIGN KEY (Owner_ID) REFERENCES Login (User_ID) ON DELETE CASCADE;
ALTER TABLE Quiz_Result ADD CONSTRAINT Result_Quiz_ID FOREIGN KEY (Quiz_ID) REFERENCES Quiz (Quiz_ID) ON DELETE CASCADE;
ALTER TABLE Answer_Sheet ADD CONSTRAINT Answer_Sheet_Result_ID FOREIGN KEY (Result_ID) REFERENCES Quiz_Result (Result_ID) ON DELETE CASCADE;
ALTER TABLE Answer_Sheet ADD CONSTRAINT Answer_Sheet_Question_ID FOREIGN KEY (Question_ID) REFERENCES Question (Question_ID) ON DELETE CASCADE;
ALTER TABLE Answer_Sheet ADD CONSTRAINT Answer_Sheet_Option_ID FOREIGN KEY (Option_ID) REFERENCES Answer_Option (Option_ID) ON DELETE CASCADE;

----------------------------------------
-- Create or replace triggers
----------------------------------------
/* CREATE OR REPLACE TRIGGER Topic_Total_Questions_Count
AFTER INSERT OR DELETE ON Question 
FOR EACH ROW 
BEGIN
  If Inserting Then
    UPDATE Topic
    SET Total_Questions = (Total_Questions + 1)
    WHERE Topic_ID = :NEW.Topic_ID;
  ELSIF Deleting THEN
    UPDATE Topic
    SET Total_Questions = (Total_Questions - 1)
    WHERE Topic_ID = :OLD.Topic_ID;
  END IF;
END;
/ */

