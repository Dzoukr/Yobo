create table OnlineLessons
(
    Id          uniqueidentifier not null
        constraint PK_OnlineLessons
            primary key,
    Name        nvarchar(max)    not null,
    Description nvarchar(max)    not null,
    StartDate   datetimeoffset   not null,
    EndDate     datetimeoffset   not null,
    Created     datetimeoffset   not null,
    IsCancelled bit              not null,
    Capacity    int              not null
)
go

create table OnlineLessonReservations
(
    OnlineLessonId   uniqueidentifier not null
        constraint FK_OnlineLessonReservations_Lessons
            references OnlineLessons,
    UserId     uniqueidentifier not null
        constraint FK_OnlineLessonReservations_Users
            references Users,
    Created    datetimeoffset   not null,
    UseCredits bit              not null,
    constraint PK_OnlineLessonReservations
        primary key (OnlineLessonId, UserId)
)
go