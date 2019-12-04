let mode = 0;

function showAll() {
  mode = 0;
}

function showLast() {
  mode = 100;
}

document.addEventListener("DOMContentLoaded", async () => {
  var connection = new signalR.HubConnectionBuilder()
    .withUrl("/darknetHub")
    .build();

  const dataTemplate = {
    datasets: [
      {
        label: "Loss",
        fill: false,
        borderColor: "rgb(255, 99, 132)",
        yAxisID: "Loss",
        data: []
      },
      {
        label: "Avg. loss",
        borderColor: "rgb(75, 192, 192)",
        backgroundColor: "rgba(75, 192, 192, 0.5)",
        fill: true,
        yAxisID: "Loss",
        data: []
      },
      {
        label: "Learning rate",
        borderColor: "blue",
        fill: false,
        yAxisID: "Learning rate",
        data: []
      }
    ],
    labels: []
  };

  const data = {
    datasets: [[], [], []],
    labels: []
  };

  const config = {
    type: "line",
    options: {
      scales: {
        yAxes: [
          {
            id: "Loss",
            type: "linear",
            position: "left"
          },
          {
            id: "Learning rate",
            type: "linear",
            position: "right"
          }
        ]
      }
    }
  };

  var chart = new Chart("training-chart", config);

  connection.on(
    "updateReceived",
    (batchId, loss, avgLoss, learningRate) => {
      console.log("Update received", loss, avgLoss, learningRate);

      data.labels.push(batchId);

      data.datasets[0].push(loss);
      data.datasets[1].push(avgLoss);
      data.datasets[2].push(learningRate);

      config.data = {
        ...dataTemplate
      };

      if (mode > 0 && data.labels.length > mode) {
        config.data.labels = data.labels.slice(-mode);
        for (let i = 0; i < data.datasets.length; i++) {
          config.data.datasets[i].data = data.datasets[i].slice(-mode);
        }
      } else {
        config.data.labels = data.labels;
        for (let i = 0; i < data.datasets.length; i++) {
          config.data.datasets[i].data = data.datasets[i];
        }
      }

      chart.update();
    }
  );

  await connection.start();
  console.log("connected");

  const jobId = document.getElementById('job-id').value

  // ToDo: Request initial data

  await connection.send("Subscribe", jobId);
});
