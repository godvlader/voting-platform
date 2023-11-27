# WPF Voting Application

This project aims to create a desktop application for conducting polls, developed in C# using the Windows Presentation Framework (WPF) and Entity Framework (EF).

## Project Description
The application manages users with unique identifiers, names, email addresses, and passwords. Some users are designated as administrators, with extended access rights compared to normal users.

### Poll Features
- Each poll has a unique identifier, title, and type.
- Polls are created by users and have a boolean attribute indicating whether they are open or closed.
- The creator can register several users, including themselves, as participants.
- Polls consist of multiple choices for which participants can vote.
- Votes are recorded as "Yes", "No", or "Maybe", with respective values of 1, 0.5, and -1.
- The most popular choice(s) are determined by summing the vote values.

### Types of Polls
- **Single**: Users can vote for only one choice.
- **Multiple**: Users can vote for several choices.

### Additional Features
- Participants can post comments on polls.
- Comments are characterized by text and the date/time of creation.

## Technical Requirements
- Compatible with Visual Studio 2022 and .NET version 7.0.
- Utilizes Entity Framework and LINQ for database access (SQL Server and SQLite).
- Project must run with both SQLite and SQL Server without errors.
- Implements WPF.
- Data and view interaction via MVVM binding mechanisms.

## General Instructions
- Graphical presentation structure: a main window with dynamic tabs, each displaying a part of the data.
- Implement dynamic layouts for views (windows and user controls) to allow proper reorganization when resizing.
- Ensure data synchronization across all open tabs.
- Handle cascading impacts of modifications (e.g., deleting a client) with user alerts.
- Implement business-relevant validations and error messages.

## Non-Functional Constraints
- Compatibility with Visual Studio 2022 and .NET 7.0.
- Entity Framework with LINQ for database access.
- SQL Server and SQLite compatibility.
- WPF usage.
- PRBD_Framework integration.
- MVVM binding for data and actions.

## Getting Started
Clone and run.

