Mongodb query to remove old unconfirmed accounts

db.Users.deleteMany({
    EmailConfirmed: false,
    CreatedOn: {$gt: ISODate('2022-10-01')},
    NormalizedEmail: {$not: {$regex: "@OKSTATE.EDU"}}
})