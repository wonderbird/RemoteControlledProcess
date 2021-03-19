@CustomReadinessCheck
Feature: Custom Readiness Check
  In order to consider my own preconditions during process startup
  As a test developer using ProcessWrapper
  I want the startup to be delayed until my custom readiness check is ready.
  
  @Ignore 
  Scenario: Custom readiness check
    Given An application was started with a custom readiness check 
    When the application is ready
    Then the custom readiness check has been executed successfully