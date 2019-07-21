## Mazebot Crawler API

Parses the details returned by the [mazebot API](https://github.com/noops-challenge/mazebot/blob/master/API.md) and attempts to solve it. More info on the mazebot API can be found [here](https://github.com/noops-challenge/mazebot).

The API exposes a couple of endpoints:

### `POST /api/mazebotcrawler/solve/random`
Calls the mazebot API `https://api.noopschallenge.com/mazebot/random` to get a random maze, attempts to solve it, and posts the solution to the provided end point.<br /><br />NOTE: The response includes a `sessionId` property that can be used to query the details of the operation.

| Parameters | Description |
|------------|-------------|
| minSize (numeric) | Sent to the mazebot API as the minSize parameter. |
| maxSize (numeric) | Sent to the mazebot API as the maxSize parameter. |

### `POST /api/mazebotcrawler/race/start`
Calls the mazebot API `https://api.noopschallenge.com/mazebot/race/start` to log in, attempts to solve the maze provided, and posts the solution to the provided end point. If the solution is correct, the API retrieves the details of next maze from the response, and then attempts to solve it as well. This process continues until there are no more details of the next maze in the response.<br /><br />Since the process could take a lot of time, this endpoint does not wait for the completion. And for this reason, "status" endpoints have been provided for querying the details of the operation.<br /><br />NOTE: The response includes a `sessionId` property that can be used to query the details of the operation and/or view the result of the race.

| Request Body | Description |
|--------------|-------------|
| login (string) | Sent to the mazebot API as part of the `/mazebot/start` request. |

### `GET /api/mazebotcrawler/race/result/{sessionId}`
Retrieves the status of the race associated with the `sessionId`.

| Route | Description |
|-------|-------------|
| sessionId (string) | Identifier for looking up the result of the race. The value can be found as a property of the response from the `/api/mazecrawler/race/start` endpoint. |

### `GET /api/mazebotcrawler/session/{sessionId}/summary`
Retrieves the brief summary of an operation associated with the `sessionId`.

| Route | Description |
|-------|-------------|
| sessionId (string) | Identifier for looking up a brief summary of an operation. The value can be found as a property of the response from either the `/api/mazecrawler/solve/random` or `/api/mazecrawler/race/start` endpoints. |

### `GET /api/mazebotcrawler/session/{sessionId}/status`
Retrieves the detailed status of an operation associated with the `sessionId`.

| Route | Description |
|-------|-------------|
| sessionId (string) | Identifier for looking up the complete details of an operation. The value can be found as a property of the response from either the `/api/mazecrawler/solve/random` or `/api/mazecrawler/race/start` endpoints. |

### `GET /api/mazebotcrawler/session/{sessionId}/maze/{mazeId}`
Retrieves an image of the maze from a particular operation with `mazeId` that is associated with the `sessionId`.

| Route | Description |
|-------|-------------|
| sessionId (string) | Identifier for the session. The value can be found as a property of the response from either the `/api/mazecrawler/solve/random` or `/api/mazecrawler/race/start` endpoints. |
| mazeId (string) | Identifier for a specific operation in the session. The value can be found as a property of the response from either the `/api/mazecrawler/solve/random` or `/api/mazecrawler/race/start` endpoints. |

### `GET /api/mazebotcrawler/session/{sessionId}/solution/{mazeId}`
Retrieves an image of the solution to the maze from a particular operation with `mazeId` that is associated with the `sessionId`.

| Route | Description |
|-------|-------------|
| sessionId (string) | Identifier for the session. The value can be found as a property of the response from either the `/api/mazecrawler/solve/random` or `/api/mazecrawler/race/start` endpoints. |
| mazeId (string) | Identifier for a specific operation in the session. The value can be found as a property of the response from either the `/api/mazecrawler/solve/random` or `/api/mazecrawler/race/start` endpoints. |

### Sample

**Solve a random maze**

Request:<br />
`POST /api/mazecrawler/solve/random`

Response<br />
200
```json
{
  "sessionId": "98b46c94-668c-491d-8bf4-86c58a7c88e2",
  "mazeId": "e38c578a-6105-4dff-b024-6d41201d5bde",
  "mazePath": "/mazebot/mazes/...",
  "message": "...",
  "nextMaze": null,
  "certificate": null
}
```

**Start a race**

Request:<br />
`POST /api/mazecrawler/race/start`
```json
{
    "login": "mazebotcrawler"
}
```

Response<br />
200
```json
{
  "createdDate": "2019-06-30T21:26:20.2752586+10:00",
  "sessionId": "fd71643b-9ea4-4b23-834c-0f9a5035b3e2",
  "message": "mazebotcrawler is in the race..."
}
```

**Retrieve results of a race (in progress)**

Request:<br />
`GET /api/mazecrawler/race/result/fd71643b-9ea4-4b23-834c-0f9a5035b3e2`

Response<br />
200
```json
{
  "message": "Race is still in progress...",
  "elapsed": 0,
  "completed": "0001-01-01T00:00:00+00:00"
}
```

**Retrieve results of a race (completed)**

Request:<br />
`GET /api/mazecrawler/race/result/fd71643b-9ea4-4b23-834c-0f9a5035b3e2`

Response<br />
200
```json
{
  "message": "This certifies that mazebotcrawler completed the mazebot race in 15.421 seconds.",
  "elapsed": 15.421,
  "completed": "2019-06-30T11:26:35.469+00:00"
}
```

**Retrieve summary of an operation**

Request:<br />
`GET /api/mazecrawler/session/fd71643b-9ea4-4b23-834c-0f9a5035b3e2/summary`

Response<br />
200
```json
[
  {
    "sessionId": "fd71643b-9ea4-4b23-834c-0f9a5035b3e2",
    "mazeId": "4c2dd3d5-f498-417a-8ea0-d51919ea72ba",
    "mazePath": "/mazebot/race/...",
    "message": "...",
    "nextMaze": null,
    "certificate": "/..."
  },
  {
    "sessionId": "fd71643b-9ea4-4b23-834c-0f9a5035b3e2",
    "mazeId": "d45bcd8e-99f3-478c-b72a-c3113a0596fb",
    "mazePath": "/mazebot/race/...",
    "message": null,
    "nextMaze": "/mazebot/race/...",
    "certificate": null
  },
  {
    ...
  },
  ...,
  {
    "sessionId": "fd71643b-9ea4-4b23-834c-0f9a5035b3e2",
    "mazeId": "3beac5ae-e016-4dde-9ab1-dff1595ce48c",
    "mazePath": "/mazebot/race/...",
    "message": null,
    "nextMaze": "/mazebot/race/...",
    "certificate": null
  }
]
```

**Retrieve details of an operation**

Request:<br />
`GET /api/mazecrawler/session/98b46c94-668c-491d-8bf4-86c58a7c88e2/status`

Response<br />
200
```json
[
  {
    "createdDate": "2019-06-30T21:21:01.8342136+10:00",
    "sessionId": "98b46c94-668c-491d-8bf4-86c58a7c88e2",
    "response": {
      "sessionId": "98b46c94-668c-491d-8bf4-86c58a7c88e2",
      "mazeId": "e38c578a-6105-4dff-b024-6d41201d5bde",
      "mazePath": "/mazebot/mazes/...",
      "directionsResult": "success",
      "message": "...",
      "elapsed": 283,
      "solutionLength": "30 (min: 30)",
      "nextMaze": null,
      "certificate": null,
      "rawMaze": "{...}",
      "rawSolution": "{...}",
      "rawResult": "{...}"
    }
  }
]
```

**Retrieve maze image used by an operation**

Request:<br />
`GET /api/mazecrawler/session/98b46c94-668c-491d-8bf4-86c58a7c88e2/maze/e38c578a-6105-4dff-b024-6d41201d5bde`

Response<br />
200<br />
![maze](https://i.imgur.com/KwKYftO.png)

**Retrieve maze solution image used by an operation**

Request:<br />
`GET /api/mazecrawler/session/98b46c94-668c-491d-8bf4-86c58a7c88e2/solution/e38c578a-6105-4dff-b024-6d41201d5bde`

Response<br />
200<br />
![solution](https://i.imgur.com/oNtH2gk.gif)