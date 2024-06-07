# How to Add a New Category Button While Saving a Report on the Server using Bold Reports Designer

This project utilizes JavaScript to demonstrate the process of adding a new category button while saving a report using a sample JavaScript application.

## Requirements to run the sample

To run the sample, ensure you have the following requirements:

* [Visual Studio Code](https://code.visualstudio.com/download)

## Using the Reporting Samples

Follow these steps to utilize the reporting samples:

1. Open the html file `CreateCategory.html` in Visual Studio Code.

2. Press `F5` or click the `Run` button in Visual Studio Code to launch the application.

## Why This Sample?

The provided code snippet illustrates how to customize the Bold Reports server integration within a JavaScript application. By initializing the Bold Reports Designer with the necessary server URLs and authorization tokens, users can dynamically add a **New Category** button to the report designer's UI. This customization ensures that the button is rendered only once during the initial save action, streamlining the report-saving process.

## Implementation Details

1. **Configure Application:** Initialize the Bold Reports Designer with relevant server details.
2. **Add New Category Button:** Define the [create](https://help.boldreports.com/embedded-reporting/javascript-reporting/report-designer/api-reference/events/#create) API event function to add the **New Category** button to the UI dynamically.
![CategoryButton.png](https://support.boldreports.com/kb/agent/attachment/inline?token=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjIzODIyIiwib3JnaWQiOiIzIiwiaXNzIjoic3VwcG9ydC5ib2xkcmVwb3J0cy5jb20ifQ.l_QK7rgTnIofHNcqyMJAEYngrCvO3TaC3oZ7MxPOkB0)
3. **Handle Button Click:** Implement the function to handle the click event for the **New Category** button, triggering the **Publish As Report** dialog.
![NewCategory.png](https://support.boldreports.com/kb/agent/attachment/inline?token=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjIzODI2Iiwib3JnaWQiOiIzIiwiaXNzIjoic3VwcG9ydC5ib2xkcmVwb3J0cy5jb20ifQ.G5vGXtrGPdiNpXXbLbfJhaP04ID2uNkHLLtkurg64P0)
4. **Create Category:** Users can enter a new category name in the dialog and refresh the list to see the newly created category.
![CategoryList.png](https://support.boldreports.com/kb/agent/attachment/inline?token=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjIzODIzIiwib3JnaWQiOiIzIiwiaXNzIjoic3VwcG9ydC5ib2xkcmVwb3J0cy5jb20ifQ.IotTU8pvVdzSM0DJ6uvDrgglmwP5gDpeaTZShZeqOvY)
5. **Save Report:** Save the report into the newly created category, improving organization and accessibility.
![Output.png](https://support.boldreports.com/kb/agent/attachment/inline?token=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjIzODIxIiwib3JnaWQiOiIzIiwiaXNzIjoic3VwcG9ydC5ib2xkcmVwb3J0cy5jb20ifQ.qpPfuSwcrR7zfQFrk7-UhVplscxhZp4a9AdX8g-RX5o)

## Documentation

For comprehensive documentation on using the Bold Reports Report Server Designer control in JavaScript applications, refer to the [JavaScript documentation](https://help.boldreports.com/embedded-reporting/javascript-reporting/report-designer/server-integration/).

## Support

* If you encounter any challenges or have inquiries regarding report creation or the issues in the reports, please do not hesitate to open a [support ticket](https://support.boldreports.com/support) with us. This allows us to investigate the matter and offer assistance to resolve any issues.