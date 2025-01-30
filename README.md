#	Coverage Service Simulator
The Coverage Service (CS) is a core component of the DOMINO-E project, developed in WP3000. While the fully functional service is not yet available, a CS simulator is used during the early stages of VAS prototype development. This simulator will later be replaced with the fully implemented CS to ensure thorough validation within the testbed.

The objective of integrating VAS with the CS is to enable use cases where VAS allows End Users to request new satellite images (i.e., creating Programming Requests) and track the status of these orders (i.e., creating Event Follow-Up Requests).

For the VAS Prototype, we have developed a CS simulator that implements the User Request Manager (URM) component of the CS, as outlined in the DOMINO-X Architecture Document. The URM provides all the CS functionality required by the VAS Prototype. The simulator is deployed in a local testing environment and is used by the [VAS Prototype Chatbot](https://va.tilde.com/api/prodk8sbotdomin0/media/staging/uas.html) .

