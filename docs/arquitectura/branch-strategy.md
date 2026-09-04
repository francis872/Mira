# Branch Strategy

Simple branching model for MIRA:

main
  |
  +-- develop
         |
         +-- v1-api-inicial

## Branch purposes

- main: stable versions only.
- develop: integration branch.
- v1-api-inicial: implementation branch for the initial API version.

## Note on Git naming

Git does not allow keeping both a branch named develop and another named develop/v1-api-inicial at the same time, because develop becomes a namespace prefix.

For this repository, v1-api-inicial is used as the equivalent implementation branch under develop workflow.
